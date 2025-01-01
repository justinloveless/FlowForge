using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

internal class InMemoryWorkflowEventQueue(IServiceProvider serviceProvider) : IWorkflowEventQueuePublisher
{
    private readonly ConcurrentQueue<(string, string, Dictionary<string, object>)> _queue = new();

    public Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        _queue.Enqueue((workflowInstanceId, eventName, eventData));
        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();
        engine.HandleEventAsync(workflowInstanceId, eventName, eventData);
        return Task.CompletedTask;
    }
}