namespace WorkflowEngine.Core;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<WorkflowEvent> _events = new();

    public Task AddEventAsync(WorkflowEvent workflowEvent)
    {
        _events.Add(workflowEvent);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<WorkflowEvent>> GetEventsAsync(Guid workflowInstanceId, string? eventType = null)
    {
        var events = _events.Where(e => e.WorkflowInstanceId == workflowInstanceId);

        if (!string.IsNullOrEmpty(eventType))
        {
            events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(events.AsEnumerable());
    }
}
