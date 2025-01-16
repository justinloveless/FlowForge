namespace FlowForge;

public interface IWorkflowEngine
{
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow);
    public Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData, string? eventName = null);
    public Task ProcessStateAsync(WorkflowInstance instance, StateDefinition currentStateDefinition, 
        WorkflowDefinition? workflowDefinition = null, string? eventName = null);
    Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId, string state = null);

    public Task TriggerEventForWorkflowDefinitionAsync(WorkflowDefinitionId definitionId, string eventName,
        Dictionary<string, object> eventData);
    Task TriggerGlobalEventAsync(string eventName, Dictionary<string, object> eventData);
    public Task HandleEventAsync(WorkflowInstanceId? instanceId, string eventName, object eventData);
}