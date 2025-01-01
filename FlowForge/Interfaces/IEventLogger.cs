namespace FlowForge;

public interface IEventLogger
{
    Task LogEventAsync(string eventType, WorkflowInstanceId? instanceId, string details);
}