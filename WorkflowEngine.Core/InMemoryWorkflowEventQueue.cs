using System.Collections.Concurrent;

namespace WorkflowEngine.Core;

internal class InMemoryWorkflowEventQueue : IWorkflowEventQueue
{
    private readonly ConcurrentQueue<(string, string, Dictionary<string, object>)> _queue = new();
    public Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        _queue.Enqueue((workflowInstanceId, eventName, eventData));
        return Task.CompletedTask;
    }

    public Task<(string WorkflowInstanceId, string EventName, Dictionary<string, object> EventData)?> WaitForEventAsync()
    {
        while (_queue.TryDequeue(out var result))
        {
            return Task.FromResult<(string, string, Dictionary<string, object>)?>(result);
        }
        return Task.FromResult<(string, string, Dictionary<string, object>)?>(null);
        
    }
}