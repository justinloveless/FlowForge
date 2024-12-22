namespace WorkflowEngine.Core;

public interface IMessageQueue
{
    Task EnqueueAsync(QueueMessage message);
    Task<QueueMessage?> DequeueAsync();
    Task AcknowledgeAsync(QueueMessage message);
}