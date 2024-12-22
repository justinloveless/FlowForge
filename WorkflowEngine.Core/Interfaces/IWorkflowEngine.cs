namespace WorkflowEngine.Core;

public interface IWorkflowEngine
{
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow);
    public Task<Guid> StartWorkflowAsync(Guid workflowId, Dictionary<string, object> initialData);
    public Task ProcessStateAsync(WorkflowInstance instance, string? eventName = null);
    Task TriggerEventAsync(Guid instanceId, string eventName, Dictionary<string, object> eventData);
}