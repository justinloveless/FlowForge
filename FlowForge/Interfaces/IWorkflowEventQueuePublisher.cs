namespace FlowForge;

public interface IWorkflowEventQueuePublisher
{
    Task PublishEventAsync(WorkflowInstanceId? workflowInstanceId, string eventName, Dictionary<string, object> eventData);
}