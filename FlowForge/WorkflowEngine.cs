using System.Runtime.CompilerServices;
using System.Text.Json;
using Jint;

[assembly: InternalsVisibleTo("Tests")]
namespace FlowForge;

//Make internal methods visible to the Tests project
internal class WorkflowEngine(
    IWorkflowRepository repository,
    IEventLogger eventLogger,
    IWorkflowEventQueuePublisher eventQueuePublisher,
    IEventRepository eventRepository,
    IDataProvider dataProvider,
    VariableUrlMappings variableUrlMappings,
    WorkflowEngineOptions workflowOptions,
    IAssignmentResolver assignmentResolver,
    IServiceProvider serviceProvider,
    WorkflowActionRegistry actionRegistry)
    : IWorkflowEngine
{
    private readonly WorkflowEngineOptions _workflowOptions = workflowOptions;

    public async Task HandleEventAsync(string instanceId, string eventName, object eventData)
    {
        var workflowInstance = await repository.GetWorkflowInstanceAsync(new WorkflowInstanceId(Guid.Parse(instanceId)));
        if (workflowInstance == null)
        {
            // Log and ignore
            Console.WriteLine($"Workflow instance {instanceId} not found.");
            return;
        }
        
        var currentState = workflowInstance.CurrentState;
        var stateDefinition = repository.GetWorkflowDefinitionAsync(workflowInstance.Id).Result.States
            .FirstOrDefault(s => s.Name == currentState);
        if (stateDefinition == null)
        {
            Console.WriteLine($"State {currentState} not found for workflow instance {instanceId}.");
            return;
        }
        
        var matchingTransition = stateDefinition.Transitions.FirstOrDefault(t => EvaluateCondition(t.Condition, workflowInstance, eventName).Result);
        if (matchingTransition != null)
        {
            // Transition to the next state
            var previousState = workflowInstance.CurrentState;
            await TransitionStateAsync(workflowInstance, matchingTransition.NextState);
                
            await LogEvent("StateTransitioned", workflowInstance.Id,
                $"From {previousState} to {workflowInstance.CurrentState}. Condition met: {matchingTransition.Condition}.", 
                workflowInstance.CurrentState);

            // recursively process the next state
            await ProcessStateAsync(workflowInstance);
        }
        
    }

    public async Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        var errorState = new ErrorState();
        workflow.States.Add(errorState); // always add a default error state
        await repository.RegisterWorkflowAsync(workflow);
        await LogEvent("WorkflowRegistered", null, $"Workflow {workflow.Name} registered");

    }

    public async Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData)
    {
        var instance = await repository.StartWorkflowAsync(workflowId, initialData);
        await LogEvent("WorkflowStarted", instance.Id, $"Workflow {workflowId} started with ID {instance.Id}", instance.CurrentState);
        await ProcessStateAsync(instance);
        return instance.Id;
    }

    public async Task ProcessStateAsync(WorkflowInstance instance, string? eventName = null)
    {
        try
        {
            var workflowDefinition = await repository.GetWorkflowDefinitionAsync(instance.Id);
            var currentStateDefinition =
                workflowDefinition?.States.FirstOrDefault(x => x.Name == instance.CurrentState);

            if (currentStateDefinition == null)
            {
                throw new InvalidOperationException(
                    $"State {instance.CurrentState} not found in workflow {workflowDefinition.Name}");
            }
            
            // execute OnEnterActions
            foreach (var action in currentStateDefinition.OnEnterActions)
            {
                var actionToExecute = actionRegistry.Create(action.Type, action.Parameters);
                await actionToExecute.ExecuteAsync(instance, action.Parameters, serviceProvider);
            }

            instance.StateData["event"] = eventName;
            foreach (var transition in currentStateDefinition.Transitions)
            {
                if (await EvaluateCondition(transition.Condition, instance, eventName) ==
                    false) continue;

                var previousState = instance.CurrentState;
                await TransitionStateAsync(instance, transition.NextState);
                
                await LogEvent("StateTransitioned", instance.Id,
                    $"From {previousState} to {instance.CurrentState}. Condition met: {transition.Condition}.", 
                    instance.CurrentState);

                // recursively process the next state
                await ProcessStateAsync(instance);
                return;
            }

            await LogEvent("StateProcessed", instance.Id,
                $"State {instance.CurrentState} processed with no transitions", 
                instance.CurrentState);
        }
        catch (Exception e)
        {
            var previousState = instance.CurrentState;
            instance.CurrentState = "Error";
            instance.StateData["previousState"] = previousState;
            await repository.UpdateWorkflowInstanceAsync(instance);
            await LogEvent("ExceptionOccured", instance.Id, 
                $"Exception occured in ProcessStateAsync: {e.Message}", 
                instance.CurrentState);
            throw;
        }
    }

    private async Task TransitionStateAsync(WorkflowInstance instance, string targetStateName)
    {
        var currentState = instance.CurrentState;
        var definition = await repository.GetWorkflowDefinitionAsync(instance.Id);
        var targetState = definition.States.FirstOrDefault(s => s.Name == targetStateName);

        if (targetState == null)
        {
            throw new InvalidOperationException($"State '{targetStateName}' does not exist in the workflow.");
        }
        
        // Execute OnExit actions for the current state
        var currentStateDef = definition.States.FirstOrDefault(s => s.Name == currentState);
        if (currentStateDef != null)
        {
            foreach (var action in currentStateDef.OnExitActions)
            {
                var actionToExecute = actionRegistry.Create(action.Type, action.Parameters);
                await actionToExecute.ExecuteAsync(instance, action.Parameters, serviceProvider);
            }
        }
        
        // Update the state
        instance.CurrentState = targetStateName;
        await repository.UpdateWorkflowInstanceAsync(instance);
    }

    private async Task LogEvent(string eventName, WorkflowInstanceId? instanceId, string details, string? currentState = "")
    {
        await eventLogger.LogEventAsync(eventName, instanceId, details);

        await eventRepository.AddEventAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instanceId ?? new WorkflowInstanceId(Guid.Empty),
            EventType = eventName,
            CurrentState = currentState,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId)
    {
        var instance = await repository.GetWorkflowInstanceAsync(instanceId);
        if (instance == null)
            throw new InvalidOperationException($"No workflow instance found with ID '{instanceId}'.");

        if (!await CanUserActOnStateAsync(actorId, instance))
        {
            await LogEvent("UnauthorizedActorTriggeredEvent", instanceId, 
                $"Event: {eventName}. ActorId: {actorId}. EventData: {JsonSerializer.Serialize(eventData)}",
                instance.CurrentState);
            return;
        }
        await LogEvent(eventName, instanceId, 
            $"External event {eventName} triggered. EventData: {JsonSerializer.Serialize(eventData)}", 
            instance.CurrentState);

        foreach (var key in eventData.Keys)
        {
            instance.StateData[key] = eventData[key];
        }
        
        await repository.UpdateWorkflowInstanceAsync(instance);
        eventQueuePublisher.PublishEventAsync(instance.Id.ToString(), eventName, eventData);
        
    }

    internal async Task<bool> CanUserActOnStateAsync(string userId, WorkflowInstance instance)
    {
        
        var workflowDefinition = await repository.GetWorkflowDefinitionAsync(instance.Id);
        var currentState = workflowDefinition?.States.FirstOrDefault(s => s.Name == instance.CurrentState);

        if (currentState == null)
        {
            throw new InvalidOperationException($"State {instance.CurrentState} not found in workflow {instance.WorkflowName}.");
        }

        return await assignmentResolver.CanActOnStateAsync(currentState.Name, instance.Id, userId);
    }

    internal async Task<bool> EvaluateCondition(string condition, WorkflowInstance? instance, string? eventName = null)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty.");

        var jintInterpreter = new Engine();

        // register workflow data
        foreach (var kvp in instance?.WorkflowData)
        {
            jintInterpreter.SetValue(kvp.Key, kvp.Value);
        }
        
        // Register variables from the state data
        foreach (var kvp in instance?.StateData)
        {
            jintInterpreter.SetValue(kvp.Key, kvp.Value);
        }

        // always set system variables after state data so they don't get overriden
        if (eventName is not null)
        {
            jintInterpreter.SetValue("event", eventName);
        }
        
        // lastly, try to fetch variables via data providers
        var extractedIdentifiers = IdentifierExtractor.DetectIdentifiers(condition);
        var unknownIdentifiers = extractedIdentifiers
            .Where(id => jintInterpreter.GetValue(id).IsUndefined())
            .ToList();
        var fetchedVariables = await FetchMissingVariablesAsync(
            unknownIdentifiers, 
            instance?.Id ?? new WorkflowInstanceId(Guid.Empty), 
            instance?.StateData);
        foreach (var variable in fetchedVariables)
        {
            var parsedValue = ParseJsonValue(variable.Value.ToString());
            jintInterpreter.SetValue(variable.Key, parsedValue);
        }
        

        try
        {
            // Evaluate the condition
            var result = jintInterpreter.Evaluate(condition);
            return Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            // Log and handle errors
            LogEvent("ConditionEvalFailure", instance.Id, 
                $"Condition evaluation failed: {condition}. Error: {ex.Message}", 
                instance.CurrentState);
            throw;
        }
    }
    private object ParseJsonValue(string jsonString)
    {
        if (!IsJson(jsonString))
            throw new JsonException($"JSON string is not a valid JSON. Received: {jsonString}");
        
        using var document = JsonDocument.Parse(jsonString);
        return document.ConvertJsonToDictionary();
    }

    
    private async Task<Dictionary<string, object>> FetchMissingVariablesAsync(
        IEnumerable<string> variables,
        WorkflowInstanceId instanceId,
        Dictionary<string, object> stateData)
    {
        var fetchedData = new Dictionary<string, object>();

        foreach (var variable in variables)
        {
            // Skip variables already in the state data
            if (stateData.ContainsKey(variable))
                continue;

            // Query data providers for the missing variable
            var url = variableUrlMappings.GetUrl(variable);
            if (url is null)
            {
                throw new InvalidOperationException($"No URL mapping found for variable '{variable}'.");
            }
            
            var value = await dataProvider.GetDataAsync(url, instanceId, stateData);
            
            fetchedData[variable] = value;
        }

        return fetchedData;
    }
    
    private bool IsJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        // Check if the string starts and ends with JSON-specific characters
        if ((input.StartsWith("{") && input.EndsWith("}")) || // JSON object
            (input.StartsWith("[") && input.EndsWith("]")) || // JSON array
            (input.StartsWith("\"") && input.EndsWith("\"")) || // JSON string
            input.Equals("null", StringComparison.OrdinalIgnoreCase) || // JSON null
            input.Equals("true", StringComparison.OrdinalIgnoreCase) || // JSON boolean
            input.Equals("false", StringComparison.OrdinalIgnoreCase)) // JSON boolean
        {
            try
            {
                JsonDocument.Parse(input);
                return true; // Successfully parsed, it's JSON
            }
            catch
            {
                // Failed to parse; it's not valid JSON
                return false;
            }
        }

        return false; // Not JSON
    }


    private static void DoNothing()
    {
        
    }

}