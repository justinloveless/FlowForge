namespace WorkflowEngine.Core;

public interface IEventLogger
{
    Task LogEventAsync(string eventType, Guid? instanceId, string details);
}