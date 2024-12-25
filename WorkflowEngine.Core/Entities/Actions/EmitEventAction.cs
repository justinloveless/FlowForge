using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class EmitEventAction(string eventType, Dictionary<string, object> eventData) : IWorkflowAction
{
    private string _eventType { get; } = eventType;
    private Dictionary<string, object> _eventData { get; } = eventData;
    private static string _type => "EventEmitter";

    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        var eventQueue = serviceProvider.GetRequiredService<IWorkflowEventQueue>();
        await eventQueue.PublishEventAsync(instance.Id.ToString(), _eventType, _eventData);
        
        var eventLogDetails =
            $"State: {instance.CurrentState}, Event Type Emitted: {_eventType}, EventData: {JsonSerializer.Serialize(eventData)}";
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