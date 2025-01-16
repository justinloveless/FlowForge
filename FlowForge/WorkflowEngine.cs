using System.Runtime.CompilerServices;
using System.Text.Json;
using FlowForge.Enums;

[assembly: InternalsVisibleTo("Tests")]
namespace FlowForge;

//Make internal methods visible to the Tests project
internal class WorkflowEngine(
    IWorkflowRepository repository,
    IEventLogger eventLogger,
    IWorkflowEventQueuePublisher eventQueuePublisher,
    IEventRepository eventRepository,
    WorkflowEngineOptions workflowOptions,
    IAssignmentResolver assignmentResolver,
    IServiceProvider serviceProvider,
    WorkflowActionRegistry actionRegistry,
    IConditionEngine conditionEngine)
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
        
            if (!await DependenciesSatisfied(workflowInstance, stateDefinition)) continue;
            
            var matchingTransition = stateDefinition.Transitions.FirstOrDefault(t => 
                conditionEngine.EvaluateCondition(t.Condition, workflowInstance, stateDefinition.Name, eventName).Result);
            if (matchingTransition == null) continue;
            
            // Transition to the next state
            await TransitionStateAsync(workflowInstance, stateDefinition, matchingTransition.NextState, workflowDefinition, matchingTransition.Condition);
            var newState = workflowDefinition.States.FirstOrDefault(s => s.Name == matchingTransition.NextState);
            if (newState == null) continue;
            // recursively process the next state
            await ProcessStateAsync(workflowInstance, newState, workflowDefinition);
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

            var shouldStart = initialState.Transitions.Any( t => conditionEngine.EvaluateCondition(t.Condition, 
                null, initialState.Name, eventName).Result);
            if (!shouldStart) continue;
            
            await eventLogger.LogEventAsync(StandardEvents.WorkflowStartedByEvent.ToString(), null, workflowDefinition.Id, 
                $"Workflow {workflowDefinition.Name} started by event {eventName}", activeStates: [initialState.Name]);
            var initialData = eventData as Dictionary<string, object> ?? new Dictionary<string, object>();
            await StartWorkflowAsync(workflowDefinition, initialData, eventName);

        }
    }

    public async Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        var errorState = new ErrorState();
        workflow.States.Add(errorState); // always add a default error state
        await repository.RegisterWorkflowAsync(workflow);
        await eventLogger.LogEventAsync(StandardEvents.WorkflowRegistered.ToString(), null, workflow.Id, 
            $"Workflow {workflow.Name} registered");

    }

    public async Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData, string? eventName = null)
    {
        var workflowDefinition = await repository.GetWorkflowDefinitionAsync(workflowId);
        return await StartWorkflowAsync(workflowDefinition, initialData, eventName);
    }

    private async Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinition workflowDefinition, Dictionary<string, object> initialData, string? eventName = null)
    {
        var instance = await repository.StartWorkflowAsync(workflowDefinition.Id, initialData);
        var initialState = workflowDefinition.States.FirstOrDefault(s => s.Name == workflowDefinition.InitialState);
        await eventLogger.LogEventAsync(StandardEvents.WorkflowStarted.ToString(), instance.Id, instance.DefinitionId,
            $"Workflow {workflowDefinition.Id} started with ID {instance.Id}", activeStates: instance.ActiveStates);
        await ProcessStateAsync(instance, initialState, workflowDefinition, eventName);
        return instance.Id;
    }

    public async Task ProcessStateAsync(WorkflowInstance instance, StateDefinition? currentStateDefinition, 
        WorkflowDefinition? workflowDefinition, string? eventName = null)
    {
        try
        {
            // check state dependencies
            if (!await DependenciesSatisfied(instance, currentStateDefinition)) return;
            
            // execute OnEnterActions
            await ExecuteOnEnterActions(instance, currentStateDefinition);

            if (eventName != null) instance.StateData["event"] = eventName;

            var transitionsCount = 0;
            foreach (var transition in currentStateDefinition.Transitions)
            {
                if (await conditionEngine.EvaluateCondition(transition.Condition, instance, currentStateDefinition.Name, eventName) ==
                    false) continue;

                await TransitionStateAsync(instance, currentStateDefinition, transition.NextState,
                    workflowDefinition, transition.Condition);
                transitionsCount++;
                var newState = workflowDefinition?.States.FirstOrDefault(s => s.Name == transition.NextState);
                if (newState == null) continue;
                // recursively process the next state
                await ProcessStateAsync(instance, newState, workflowDefinition);
            }

            if (transitionsCount == 0)
            {
                await eventLogger.LogEventAsync(StandardEvents.StateProcessed.ToString(), instance.Id, instance.DefinitionId,
                    $"State {currentStateDefinition.Name} processed with no transitions", currentStateDefinition.Name,
                    instance.ActiveStates);
            }
        }
        catch (Exception e)
        {
            var previousStates = instance.ActiveStates;
            instance.ActiveStates = ["Error"];
            instance.StateData["previousStates"] = previousStates;
            await repository.UpdateWorkflowInstanceAsync(instance);
            await eventLogger.LogEventAsync(StandardEvents.ExceptionOccured.ToString(), instance.Id, instance.DefinitionId,
                $"Exception occured in ProcessStateAsync: {e.Message}", 
                currentStateDefinition.Name,
                instance.ActiveStates);
            throw;
        }
    }

    private async Task ExecuteOnEnterActions(WorkflowInstance instance, StateDefinition currentStateDefinition)
    {
        foreach (var action in currentStateDefinition.OnEnterActions)
        {
            var actionToExecute = actionRegistry.Create(action.Type, action.Parameters);
            await actionToExecute.ExecuteAsync(instance, action.Parameters, serviceProvider);
        }
    }

    private async Task<bool> DependenciesSatisfied(WorkflowInstance instance, StateDefinition currentStateDefinition)
    {
        if (currentStateDefinition.DependsOn?.Count <= 0) return false;
        // get states that have been deactivated
        var deactivatedStates = (await eventRepository
                .GetEventsAsync(instance.Id, StandardEvents.StateTransitioned.ToString()))
            .Select(e => e.SourceState).Distinct();
        var currentlyActiveStates = instance.ActiveStates;
        // if we depend on a state to be completed, it should exist in the deactivatedStates
        // AND not exist in the currently active states (because it could have been reactivated)
        var satisfied = currentStateDefinition.DependsOn?
            .All(dep => deactivatedStates.Contains(dep) && !currentlyActiveStates.Contains(dep));
        if (satisfied == true) 
            await eventLogger.LogEventAsync(StandardEvents.DependenciesSatisfied.ToString(), instance.Id, instance.DefinitionId, 
                details: $"Dependent states [{string.Join(", ", currentStateDefinition.DependsOn ?? [])}] have all been completed.", 
                currentStateDefinition.Name, instance.ActiveStates);

        return satisfied == true;
    }

    private async Task TransitionStateAsync(WorkflowInstance instance, StateDefinition? currentState, string targetStateName,
        WorkflowDefinition? workflowDefinition, string? conditionMet = null)
    {
        var targetStateExists = workflowDefinition.States.Any(s => s.Name == targetStateName);
        
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
        if (currentState?.Name != targetStateName)
        {
            if (!instance.ActiveStates.Contains(targetStateName) && targetStateExists)
                instance.ActiveStates.Add(targetStateName);
            instance.ActiveStates.Remove(currentState?.Name);
            await repository.UpdateWorkflowInstanceAsync(instance);
            await eventLogger.LogEventAsync(StandardEvents.StateTransitioned.ToString(), instance.Id, instance.DefinitionId,
                $"{(targetStateExists 
                        ? $"Activated state {targetStateName}." 
                        : $"Target state '{targetStateName}' does not exist in the workflow. Target state was not activated."
                    )} Deactivated state {currentState.Name}. Condition met: {conditionMet}.", 
                currentState.Name, instance.ActiveStates);
        }
        
    }

    public async Task TriggerGlobalEventAsync(string eventName, Dictionary<string, object> eventData)
    {
        await eventLogger.LogEventAsync(eventName, null, null,
            $"External global event {eventName} triggered. EventData: {JsonSerializer.Serialize(eventData)}");
        eventQueuePublisher.PublishEventAsync(null, eventName, eventData);
        
    }
    
    public async Task TriggerEventForWorkflowDefinitionAsync(WorkflowDefinitionId definitionId, string eventName, Dictionary<string, object> eventData)
    {
        var instances = await repository.GetWorkflowInstancesByDefinitionIdAsync(definitionId);
        var tasks = instances.Select(instance => TriggerEventAsync(instance.Id, eventName, eventData, actorId: "system")).ToList();
        await Task.WhenAll(tasks);
    }
    
    public async Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId, string state = null)
    {
        var instance = await repository.GetWorkflowInstanceAsync(instanceId);
        if (instance == null)
            throw new InvalidOperationException($"No workflow instance found with ID '{instanceId}'.");
    
        var workflowDefinition = await repository.GetWorkflowDefinitionAsync(instance.Id);
        var activeStates = workflowDefinition.States.Where(s => 
            (state != null && s.Name == state) || instance.ActiveStates.Contains(s.Name));
        if (!activeStates.Any(s => assignmentResolver.CanActOnStateAsync(s.Name, instance.Id, actorId).Result))
        {
            await eventLogger.LogEventAsync(StandardEvents.UnauthorizedActorTriggeredEvent.ToString(), instanceId, instance.DefinitionId,
                $"Event: {eventName}. ActorId: {actorId}. EventData: {JsonSerializer.Serialize(eventData)}",
                activeStates: instance.ActiveStates);
            return;
        }
        await eventLogger.LogEventAsync(eventName, instanceId, instance.DefinitionId,
            $"External event {eventName} triggered. EventData: {JsonSerializer.Serialize(eventData)}", 
            activeStates: instance.ActiveStates);
    
        foreach (var key in eventData.Keys)
        {
            instance.StateData[key] = eventData[key];
        }
        
        await repository.UpdateWorkflowInstanceAsync(instance);
        eventQueuePublisher.PublishEventAsync(instance.Id, eventName, eventData);
        
    }
}