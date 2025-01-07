using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge;

internal class InMemoryWorkflowEventQueue(IServiceProvider serviceProvider) : IWorkflowEventQueuePublisher
{
    private readonly ConcurrentQueue<(WorkflowInstanceId?, string, Dictionary<string, object>)> _queue = new();

    public Task PublishEventAsync(WorkflowInstanceId? workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        _queue.Enqueue((workflowInstanceId, eventName, eventData));
        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();
        engine.HandleEventAsync(workflowInstanceId, eventName, eventData);
        return Task.CompletedTask;
    }
}