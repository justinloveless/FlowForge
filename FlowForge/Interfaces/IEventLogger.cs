namespace FlowForge;

public interface IEventLogger
{
    Task<WorkflowEventId> LogEventAsync(string eventType, WorkflowInstanceId? instanceId, WorkflowDefinitionId? definitionId, 
        string details, string? sourceState = null, List<string>? activeStates = null);
}