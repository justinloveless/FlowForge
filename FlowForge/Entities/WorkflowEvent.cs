namespace FlowForge;

public class WorkflowEvent
{
    public WorkflowEvent()
    {
        
    }
    public WorkflowEvent(string eventName, WorkflowInstanceId? instanceId,
        WorkflowDefinitionId? definitionId, string details, string? sourceState = null,
        List<string> activeStates = null)
    {
        WorkflowInstanceId = instanceId ?? new WorkflowInstanceId(Guid.Empty);
        WorkflowDefinitionId = definitionId ?? new WorkflowDefinitionId(Guid.Empty);
        EventType = eventName;
        ActiveStates = [..activeStates];
        SourceState = sourceState ?? string.Empty;
        Details = details;
        Timestamp = DateTime.UtcNow;
    }

    public WorkflowEvent(string eventName, WorkflowInstance instance, string details, string? sourceState = null)
    {
        WorkflowInstanceId = instance.Id;
        WorkflowDefinitionId = instance.DefinitionId;
        EventType = eventName;
        SourceState = sourceState ?? string.Empty;
        ActiveStates = instance.ActiveStates;
        Details = details;
        Timestamp = DateTime.UtcNow;
    }
    public WorkflowEventId Id { get; init; } = new(Guid.NewGuid());
    public WorkflowInstanceId WorkflowInstanceId { get; init; }
    public WorkflowDefinitionId WorkflowDefinitionId { get; init; }
    public string EventType { get; init; } = string.Empty; // e.g., "StateTransition", "WebhookCall"
    public string SourceState { get; set; } = string.Empty;
    public List<string> ActiveStates { get; set; } = [];
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty; // JSON or human-readable details
}

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct WorkflowEventId;