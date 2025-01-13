using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge;

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
        
        await eventQueue.PublishEventAsync(instance.Id, eventType.ToString(), eventData);
        
        var eventLogDetails =
            $"Active states: {string.Join(", ", instance.ActiveStates)}, Event Type Emitted: {eventType}, EventData: {JsonSerializer.Serialize(eventData)}";
        await eventLogger.LogEventAsync($"{_type}Executed", instance.Id, instance.DefinitionId, eventLogDetails, activeStates: instance.ActiveStates);

        await eventRepository.AddEventAsync(new WorkflowEvent($"{_type}Executed", instance, eventLogDetails));
    }
}