using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class CustomBehaviorAction(string type, Func<WorkflowInstance,  IServiceProvider, Task> customBehavior)
    : IWorkflowAction
{
    private Func<WorkflowInstance, IServiceProvider, Task> _customBehavior { get; } = customBehavior;
    private string _type { get; } = type;

    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        _customBehavior(instance, serviceProvider);
        
        var eventLogDetails =
            $"Custom behavior {_type} executed. State: {instance.CurrentState}, Parameters: {JsonSerializer.Serialize(parameters)}";
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