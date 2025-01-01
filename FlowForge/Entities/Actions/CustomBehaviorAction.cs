using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge;

public class CustomBehaviorAction
    : WorkflowAction, IWorkflowAction
{
    public CustomBehaviorAction(string type, Func<WorkflowInstance, IDictionary<string, object>, IServiceProvider, Task> customBehavior)
    {
        Type = type;
        Parameters = new Dictionary<string, object>();
        _customBehavior = customBehavior;
    }
    private Func<WorkflowInstance, IDictionary<string, object>, IServiceProvider, Task> _customBehavior { get; }

    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        _customBehavior(instance, parameters, serviceProvider);
        
        var eventLogDetails =
            $"Custom behavior {Type} executed. State: {instance.CurrentState}, Parameters: {JsonSerializer.Serialize(parameters)}";
        await eventLogger.LogEventAsync($"{Type}Executed", instance.Id, eventLogDetails);

        await eventRepository.AddEventAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instance.Id,
            EventType = $"{Type}Executed",
            CurrentState = instance.CurrentState,
            Details = eventLogDetails,
            Timestamp = DateTime.UtcNow
        });
    }
}