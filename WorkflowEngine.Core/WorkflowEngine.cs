using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DynamicExpresso;
using DynamicExpresso.Exceptions;

[assembly: InternalsVisibleTo("UnitTests")]
namespace WorkflowEngine.Core;

//Make internal methods visible to the UnitTests project
public class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowRepository _repository;
    private readonly IWebhookHandler _webhookHandler;
    private readonly IEventLogger _eventLogger;
    private readonly IWorkflowEventQueue _eventQueue;
    private readonly IEventRepository _eventRepository;
    private readonly IDataProvider _dataProvider;
    private readonly VariableUrlMappings _variableUrlMappings;

    public WorkflowEngine(
        IWorkflowRepository repository,
        IWebhookHandler webhookHandler, 
        IEventLogger eventLogger, 
        IWorkflowEventQueue eventQueue,
        IEventRepository eventRepository,
        IDataProvider dataProvider,
        VariableUrlMappings variableUrlMappings)
    {
        _repository = repository;
        _webhookHandler = webhookHandler;
        _eventLogger = eventLogger;
        _eventQueue = eventQueue;
        _eventRepository = eventRepository;
        _dataProvider = dataProvider;
        _variableUrlMappings = variableUrlMappings;
    }

    public async Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        await _repository.RegisterWorkflowAsync(workflow);
        await LogEvent("WorkflowRegistered", null, $"Workflow {workflow.Name} registered");

    }

    public async Task<Guid> StartWorkflowAsync(Guid workflowId, Dictionary<string, object> initialData)
    {
        var instance = await _repository.StartWorkflowAsync(workflowId, initialData);
        await LogEvent("WorkflowStarted", instance.Id, $"Workflow {workflowId} started with ID {instance.Id}");
        await ProcessStateAsync(instance);
        return instance.Id;
    }

    public async Task ProcessStateAsync(WorkflowInstance instance, string? eventName = null)
    {
        var workflowDefinition = await _repository.GetWorkflowDefinitionAsync(instance.Id);
        var currentStateDefinition = workflowDefinition?.States.FirstOrDefault(x => x.Name == instance.CurrentState);

        if (currentStateDefinition == null)
        {
            throw new InvalidOperationException($"State {instance.CurrentState} not found in workflow {workflowDefinition.Name}");
        }

        if (eventName is null || currentStateDefinition.TriggerWebhookOnExternalEvent)
        {
            var updatedStateData = await _webhookHandler.CallWebhookAsync(currentStateDefinition.Webhook, instance);
            instance.StateData = updatedStateData;
            await LogEvent("WebhookCalled", instance.Id, $"State: {instance.CurrentState}, Webhook: {currentStateDefinition.Webhook}");
            
        }

        if (currentStateDefinition.IsIdle)
        {
            // Log that the state is idle
            await LogEvent("IdleState", instance.Id,
                $"Workflow {instance.WorkflowName} is idling in state {instance.CurrentState}.");

            // Wait for an event to proceed
            var @event = await _eventQueue.WaitForEventAsync();

            if (@event != null && @event.Value.WorkflowInstanceId == instance.Id.ToString())
            {
                // Update the state data with the event's data
                foreach (var key in @event.Value.EventData.Keys)
                {
                    instance.StateData[key] = @event.Value.EventData[key];
                }

                await _repository.UpdateWorkflowInstanceAsync(instance);
            }
            else
            {
                // Remain in the same state if the event isn't relevant
                return;
            }
        }
        
        foreach (var transition in currentStateDefinition.Transitions)
        {
            if (await EvaluateCondition(transition.Condition, instance.Id, instance.StateData, eventName) == false) continue;
            
            var previousState = instance.CurrentState;
            instance.CurrentState = transition.NextState;
                
            await _repository.UpdateWorkflowInstanceAsync(instance);
            await LogEvent("StateTransition", instance.Id, $"From {previousState} to {instance.CurrentState}");
                
            // recursively process the next state
            await ProcessStateAsync(instance);
            return;
        }
        
        await LogEvent("StateProcessed", instance.Id, $"State {instance.CurrentState} processed with no transitions");
    }

    private async Task LogEvent(string eventName, Guid? instanceId, string details)
    {
        await _eventLogger.LogEventAsync(eventName, instanceId, details);

        await _eventRepository.AddEventAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instanceId ?? Guid.Empty,
            EventType = eventName,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task TriggerEventAsync(Guid instanceId, string eventName, Dictionary<string, object> eventData)
    {
        var instance = await _repository.GetWorkflowInstanceAsync(instanceId);
        if (instance == null)
            throw new InvalidOperationException($"No workflow instance found with ID '{instanceId}'.");
        
        await _eventLogger.LogEventAsync("ExternalEventTriggered", instanceId, $"Event {eventName} triggered.");

        foreach (var key in eventData.Keys)
        {
            instance.StateData[key] = eventData[key];
        }
        
        await _repository.UpdateWorkflowInstanceAsync(instance);
        _eventQueue.PublishEventAsync(instance.Id.ToString(), eventName, eventData);
        
        await ProcessStateAsync(instance, eventName);
    }

    internal async Task<bool> EvaluateCondition(string condition, Guid instanceId, Dictionary<string, object> stateData, string? eventName = null)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty.");

        // Create a locked-down interpreter
        var interpreter = new Interpreter(InterpreterOptions.Default)
            .Reference(typeof(bool))    // Allow basic boolean operations
            .Reference(typeof(string))  // Allow string operations
            .Reference(typeof(double)); // Allow numeric operations

        // Register variables from the state data
        foreach (var kvp in stateData)
        {
            interpreter.SetVariable(kvp.Key, kvp.Value);
        }

        // always set system variables after state data so they don't get overriden
        if (eventName is not null)
            interpreter.SetVariable("event", eventName);
        
        // lastly, try to fetch variables via data providers
        var identifiers = interpreter.DetectIdentifiers(condition);
        var fetchedVariables = await FetchMissingVariablesAsync(identifiers.UnknownIdentifiers, instanceId, stateData);
        foreach (var variable in fetchedVariables)
        {
            interpreter.SetVariable(variable.Key, variable.Value);
        }
        

        try
        {
            // Evaluate the condition
            var result = interpreter.Eval(condition);
            return Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            // Log and handle errors
            Console.WriteLine($"Condition evaluation failed: {condition}. Error: {ex.Message}");
            throw;
        }
    }
    
    private async Task<Dictionary<string, object>> FetchMissingVariablesAsync(
        IEnumerable<string> variables,
        Guid instanceId,
        Dictionary<string, object> stateData)
    {
        var fetchedData = new Dictionary<string, object>();

        foreach (var variable in variables)
        {
            // Skip variables already in the state data
            if (stateData.ContainsKey(variable))
                continue;

            // Query data providers for the missing variable
            var url = _variableUrlMappings.GetUrl(variable);
            if (url is null)
            {
                throw new InvalidOperationException($"No URL mapping found for variable '{variable}'.");
            }
            
            var value = await _dataProvider.GetDataAsync(url, instanceId, stateData);
            fetchedData[variable] = value;
        }

        return fetchedData;
    }


}