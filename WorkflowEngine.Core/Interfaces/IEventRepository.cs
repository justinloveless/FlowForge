namespace WorkflowEngine.Core;

public interface IEventRepository
{
    Task AddEventAsync(WorkflowEvent workflowEvent);
    Task<IEnumerable<WorkflowEvent>> GetEventsAsync(Guid workflowInstanceId, string? eventType = null);
}