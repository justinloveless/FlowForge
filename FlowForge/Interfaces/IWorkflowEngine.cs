namespace FlowForge;

public interface IWorkflowEngine
{
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow);
    public Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData, string? eventName = null);
    public Task ProcessStateAsync(WorkflowInstance instance, string? eventName = null);
    Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId);
    Task TriggerGlobalEventAsync(string eventName, Dictionary<string, object> eventData);
    public Task HandleEventAsync(string instanceId, string eventName, object eventData);
}