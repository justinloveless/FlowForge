namespace WorkflowEngine.Core;

public interface IEventLogger
{
    Task LogEventAsync(string eventType, WorkflowInstanceId? instanceId, string details);
}