using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

internal class InMemoryWorkflowEventQueue(IServiceProvider serviceProvider) : IWorkflowEventQueuePublisher
{
    private readonly ConcurrentQueue<(string, string, Dictionary<string, object>)> _queue = new();
    // private Func<string, string, Dictionary<string, object>, IServiceProvider, Task>? _onEventReceived;

    public Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        _queue.Enqueue((workflowInstanceId, eventName, eventData));
        // _onEventReceived?.Invoke(workflowInstanceId, eventName, eventData, serviceProvider);
        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();
        engine.HandleEventAsync(workflowInstanceId, eventName, eventData);
        return Task.CompletedTask;
    }

    // public void Subscribe(Func<string, string, Dictionary<string, object>, IServiceProvider,  Task> onEventReceived)
    // {
    //     _onEventReceived = onEventReceived;
    // }
}