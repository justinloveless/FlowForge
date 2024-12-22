namespace WorkflowEngine.Core;

public interface IWorkflowEventQueue
{
    Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData);
    Task<(string WorkflowInstanceId, string EventName, Dictionary<string, object> EventData)?> WaitForEventAsync();
}