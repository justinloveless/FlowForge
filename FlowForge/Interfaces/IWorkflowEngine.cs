namespace FlowForge;

public interface IWorkflowEngine
{
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow);
    public Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData);
    public Task ProcessStateAsync(WorkflowInstance instance, string? eventName = null);
    Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId);

    public Task HandleEventAsync(string instanceId, string eventName, object eventData);
}