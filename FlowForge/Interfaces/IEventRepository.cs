namespace FlowForge;

public interface IEventRepository
{
    Task AddEventAsync(WorkflowEvent workflowEvent);
    Task<IEnumerable<WorkflowEvent>> GetEventsAsync(WorkflowInstanceId workflowInstanceId, string? eventType = null);
}