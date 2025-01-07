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

    public async Task HandleEventAsync(WorkflowInstanceId? instanceId, string eventName, object eventData)
    {
        if (instanceId is null || instanceId == new WorkflowInstanceId(Guid.Empty))
        {
            await StarWorkflowsByEvent(eventName, eventData);
            return;
        } 
        
        var workflowInstance = await repository.GetWorkflowInstanceAsync((WorkflowInstanceId)instanceId);
        if (workflowInstance == null)
        {
            // Log and ignore
            Console.WriteLine($"Workflow instance {instanceId} not found.");
            return;
        }

        var workflowDefinition = repository.GetWorkflowDefinitionAsync(workflowInstance.Id).Result;
        var activeStates = workflowDefinition.States.Where(s => 
            workflowInstance.ActiveStates.Contains(s.Name)).ToList();
        if (activeStates?.Count == 0)
        {
            Console.WriteLine($"No active states found for workflow instance {instanceId}.");
            return;
        }
        
        foreach (var stateDefinition in activeStates)
        {
        
            var matchingTransition = stateDefinition.Transitions.FirstOrDefault(t =>
                EvaluateCondition(t.Condition, workflowInstance, eventName).Result);
            if (matchingTransition == null) continue;
            
            // Transition to the next state
            await TransitionStateAsync(workflowInstance, stateDefinition, matchingTransition.NextState, workflowDefinition, matchingTransition.Condition);
            // recursively process the next state
            await ProcessStateAsync(workflowInstance, stateDefinition, workflowDefinition);
        }
        
    }

    private async Task StarWorkflowsByEvent(string eventName, object eventData)
    {
        // handle starting event-driven workflows 
        var eventDrivenWorkflows = await repository.GetEventDrivenWorkflowDefinitionsAsync(eventName);
        foreach (var workflowDefinition in eventDrivenWorkflows)      
        {
            var initialState = workflowDefinition.States.FirstOrDefault(s => s.Name == workflowDefinition.InitialState);
            if (initialState is null) continue;

            var shouldStart = initialState.Transitions.Any( t => EvaluateCondition(t.Condition, null, eventName).Result);
            if (!shouldStart) continue;
            
            await LogEvent("WorkflowStartedByEvent", null, workflowDefinition.Id, 
                $"Workflow {workflowDefinition.Name} started by event {eventName}", [initialState.Name]);
            await StartWorkflowAsync(workflowDefinition, eventData as Dictionary<string, object>, eventName);

        }
    }

    public async Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        var errorState = new ErrorState();
        workflow.States.Add(errorState); // always add a default error state
        await repository.RegisterWorkflowAsync(workflow);
        await LogEvent("WorkflowRegistered", null, workflow.Id, $"Workflow {workflow.Name} registered");

    }

    public async Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData, string? eventName = null)
    {
        var workflowDefinition = await repository.GetWorkflowDefinitionAsync(workflowId);
        return await StartWorkflowAsync(workflowDefinition, initialData, eventName);
    }
    
    public async Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinition workflowDefinition, Dictionary<string, object> initialData, string? eventName = null)
    {
        var instance = await repository.StartWorkflowAsync(workflowDefinition.Id, initialData);
        var initialState = workflowDefinition.States.FirstOrDefault(s => s.Name == workflowDefinition.InitialState);
        await LogEvent("WorkflowStarted", instance.Id, instance.DefinitionId,
            $"Workflow {workflowDefinition.Id} started with ID {instance.Id}", instance.ActiveStates);
        await ProcessStateAsync(instance, initialState, workflowDefinition, eventName);
        return instance.Id;
    }

    public async Task ProcessStateAsync(WorkflowInstance instance, StateDefinition currentStateDefinition, 
        WorkflowDefinition workflowDefinition, string? eventName = null)
    {
        try
        {
            // execute OnEnterActions
            foreach (var action in currentStateDefinition.OnEnterActions)
            {
                var actionToExecute = actionRegistry.Create(action.Type, action.Parameters);
                await actionToExecute.ExecuteAsync(instance, action.Parameters, serviceProvider);
            }

            if (eventName != null) instance.StateData["event"] = eventName;
            
            foreach (var transition in currentStateDefinition.Transitions)
            {
                if (await EvaluateCondition(transition.Condition, instance, eventName) ==
                    false) continue;

                await TransitionStateAsync(instance, currentStateDefinition, transition.NextState,
                    workflowDefinition, transition.Condition);
                var newState = workflowDefinition.States.FirstOrDefault(s => s.Name == transition.NextState);
                // recursively process the next state
                await ProcessStateAsync(instance, newState, workflowDefinition);
                return;
            }

            await LogEvent("StateProcessed", instance.Id, instance.DefinitionId,
                $"State {currentStateDefinition.Name} processed with no transitions", 
                instance.ActiveStates);
        }
        catch (Exception e)
        {
            var previousStates = instance.ActiveStates;
            instance.ActiveStates = ["Error"];
            instance.StateData["previousStates"] = previousStates;
            await repository.UpdateWorkflowInstanceAsync(instance);
            await LogEvent("ExceptionOccured", instance.Id, instance.DefinitionId,
                $"Exception occured in ProcessStateAsync: {e.Message}", 
                instance.ActiveStates);
            throw;
        }
    }

    private async Task TransitionStateAsync(WorkflowInstance instance, StateDefinition currentState, string targetStateName,
        WorkflowDefinition workflowDefinition, string? conditionMet = null)
    {
        var targetStateExists = workflowDefinition.States.Any(s => s.Name == targetStateName);

        if (!targetStateExists)
        {
            throw new InvalidOperationException($"State '{targetStateName}' does not exist in the workflow.");
        }
        
        // Execute OnExit actions for the current state
        if (currentState != null)
        {
            foreach (var action in currentState.OnExitActions)
            {
                var actionToExecute = actionRegistry.Create(action.Type, action.Parameters);
                await actionToExecute.ExecuteAsync(instance, action.Parameters, serviceProvider);
            }
        }
        
        // Update the state
        instance.ActiveStates.Add(targetStateName);
        instance.ActiveStates.Remove(currentState.Name);
        await repository.UpdateWorkflowInstanceAsync(instance);
        
        await LogEvent("StateTransitioned", instance.Id, instance.DefinitionId,
            $"Activated state {targetStateName}. Deactivated state {currentState.Name}. Condition met: {conditionMet}.", 
            instance.ActiveStates);
    }

    // private async Task ProcessForkAsync(WorkflowInstance instance, ForkState fork)
    // {
    //     foreach (var path in fork.ParallelPaths)
    //     {
    //         // add each path to active states
    //         // instance.ActiveStates.Add(path);
    //         var eventId = TriggerInternalEventAsync(instance.Id, fork.Name, 
    //             $"ActivatedState:{path}", new Dictionary<string, object>());
    //     }
    //
    //     // instance.ActiveStates.Add(fork.JoinState);
    //     await repository.UpdateWorkflowInstanceAsync(instance);
    // }
    //
    // private async Task ProcessJoinAsync(WorkflowInstance instance, JoinState join)
    // {
    //     var completedPaths = await GetCompletedPaths(instance, join.ForkEventId);
    //
    //     if (completedPaths.Count == join.WaitForEvents.Count)
    //     {
    //         var matchingTransition = join.Transitions.FirstOrDefault(t => EvaluateCondition(t.Condition, instance).Result);
    //         if (matchingTransition != null)
    //         {
    //             await TransitionStateAsync(instance, matchingTransition.NextState, matchingTransition.Condition);
    //             await ProcessStateAsync(instance);
    //         }
    //     }
    //     else
    //     {
    //         await LogEvent("JoinStateProgress", instance.Id,
    //             $"Join state waiting for completion of {string.Join(", ", join.WaitForEvents.Except(completedPaths))}");
    //     }
    // }
    //
    // private async Task<IEnumerable<string>> GetCompletedPaths(WorkflowInstance)
    // {
    //     
    // }

    private async Task<WorkflowEventId> LogEvent(string eventName, WorkflowInstanceId? instanceId, WorkflowDefinitionId? definitionId, string details, List<string> activeStates = null)
    {
        await eventLogger.LogEventAsync(eventName, instanceId, details);
        if (activeStates == null) activeStates = [];
        

        var workflowEvent = new WorkflowEvent
        {
            WorkflowInstanceId = instanceId ?? new WorkflowInstanceId(Guid.Empty),
            WorkflowDefinitionId = definitionId ?? new WorkflowDefinitionId(Guid.Empty),
            EventType = eventName,
            ActiveStates = [..activeStates],
            Details = details,
            Timestamp = DateTime.UtcNow
        };
        await eventRepository.AddEventAsync(workflowEvent);
        return workflowEvent.Id;
    }

    public async Task TriggerGlobalEventAsync(string eventName, Dictionary<string, object> eventData)
    {
        await LogEvent(eventName, null, null,
            $"External global event {eventName} triggered. EventData: {JsonSerializer.Serialize(eventData)}");
        eventQueuePublisher.PublishEventAsync(null, eventName, eventData);
        
    }

    private async Task<WorkflowEventId> TriggerInternalEventAsync(WorkflowInstanceId instanceId, WorkflowDefinitionId definitionId,
        List<string> currentState, string eventName, Dictionary<string, object> eventData)
    {
        var eventId = await LogEvent(eventName, instanceId, definitionId,
            $"Internal event {eventName} triggered. EventData: {JsonSerializer.Serialize(eventData)}", 
            currentState);
        eventQueuePublisher.PublishEventAsync(instanceId, eventName, eventData);
        return eventId;
    }

    public async Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId, string state = null)
    {
        var instance = await repository.GetWorkflowInstanceAsync(instanceId);
        if (instance == null)
            throw new InvalidOperationException($"No workflow instance found with ID '{instanceId}'.");

        var workflowDefinition = await repository.GetWorkflowDefinitionAsync(instance.Id);
        var activeStates = workflowDefinition.States.Where(s => 
            (state != null && s.Name == state) || instance.ActiveStates.Contains(s.Name));
        if (!activeStates.Any(s => CanUserActOnStateAsync(actorId, instance, s).Result))
        {
            await LogEvent("UnauthorizedActorTriggeredEvent", instanceId, instance.DefinitionId,
                $"Event: {eventName}. ActorId: {actorId}. EventData: {JsonSerializer.Serialize(eventData)}",
                instance.ActiveStates);
            return;
        }
        await LogEvent(eventName, instanceId, instance.DefinitionId,
            $"External event {eventName} triggered. EventData: {JsonSerializer.Serialize(eventData)}", 
            instance.ActiveStates);

        foreach (var key in eventData.Keys)
        {
            instance.StateData[key] = eventData[key];
        }
        
        await repository.UpdateWorkflowInstanceAsync(instance);
        eventQueuePublisher.PublishEventAsync(instance.Id, eventName, eventData);
        
    }

    internal async Task<bool> CanUserActOnStateAsync(string userId, WorkflowInstance instance, StateDefinition stateDefinition)
    {
        if (stateDefinition == null)
        {
            throw new InvalidOperationException($"State {instance.ActiveStates} not found in workflow {instance.WorkflowName}.");
        }

        return await assignmentResolver.CanActOnStateAsync(stateDefinition.Name, instance.Id, userId);
    }

    internal async Task<bool> EvaluateCondition(string condition, WorkflowInstance? instance, string? eventName = null)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty.");

        var jintInterpreter = new Engine();

        if (instance is not null)
        {
            // register workflow data
            foreach (var kvp in instance.WorkflowData)
            {
                jintInterpreter.SetValue(kvp.Key, kvp.Value);
            }
            
            // Register variables from the state data
            foreach (var kvp in instance.StateData)
            {
                jintInterpreter.SetValue(kvp.Key, kvp.Value);
            }
        }

        // always set system variables after state data so they don't get overriden
        // if (eventName is not null)
        // {
        // }
        jintInterpreter.SetValue("event", eventName);
        
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
            LogEvent("ConditionEvalFailure", instance.Id, instance.DefinitionId,
                $"Condition evaluation failed: {condition}. Error: {ex.Message}", 
                instance.ActiveStates);
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

}