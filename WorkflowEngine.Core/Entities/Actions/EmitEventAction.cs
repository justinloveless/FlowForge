using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class EmitEventAction : IWorkflowAction
{
    private static string _type => "EventEmitter";

    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        var eventQueue = serviceProvider.GetRequiredService<IWorkflowEventQueuePublisher>();
        
        
        var hasEventType = parameters.TryGetValue("eventType", out var eventType);
        if (!hasEventType)
            throw new InvalidOperationException("Must have event type in EventEmitter action");
        
        var eventDataString = parameters.TryGetValue("headers", out var eventDataObj) ? eventDataObj.ToString() : "{}";
        var eventData = JsonSerializer.Deserialize<Dictionary<string, object>>(eventDataString);
        
        await eventQueue.PublishEventAsync(instance.Id.ToString(), eventType.ToString(), eventData);
        
        var eventLogDetails =
            $"State: {instance.CurrentState}, Event Type Emitted: {eventType}, EventData: {JsonSerializer.Serialize(eventData)}";
        await eventLogger.LogEventAsync($"{_type}Executed", instance.Id, eventLogDetails);

        await eventRepository.AddEventAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instance.Id,
            EventType = $"{_type}Executed",
            CurrentState = instance.CurrentState,
            Details = eventLogDetails,
            Timestamp = DateTime.UtcNow
        });
    }
}