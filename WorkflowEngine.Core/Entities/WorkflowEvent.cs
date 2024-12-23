namespace WorkflowEngine.Core;

public class WorkflowEvent
{
    public WorkflowEventId Id { get; init; } = new(Guid.NewGuid());
    public WorkflowInstanceId WorkflowInstanceId { get; init; }
    public string EventType { get; init; } = string.Empty; // e.g., "StateTransition", "WebhookCall"
    public string CurrentState { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty; // JSON or human-readable details
}

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct WorkflowEventId;