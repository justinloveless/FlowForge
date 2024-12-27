namespace WorkflowEngine.Core;

public interface IWorkflowEventQueuePublisher
{
    Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData);
}