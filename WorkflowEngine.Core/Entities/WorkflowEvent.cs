namespace WorkflowEngine.Core;

public class WorkflowEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowInstanceId { get; set; }
    public string EventType { get; set; } = string.Empty; // e.g., "StateTransition", "WebhookCall"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty; // JSON or human-readable details
}