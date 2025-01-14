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
            $"Custom behavior {Type} executed. Active States: {string.Join(", ", instance.ActiveStates)}, Parameters: {JsonSerializer.Serialize(parameters)}";
        await eventLogger.LogEventAsync($"{Type}Executed", instance.Id, instance.DefinitionId, eventLogDetails, activeStates: instance.ActiveStates);

        await eventRepository.AddEventAsync(new WorkflowEvent($"{Type}Executed", instance, eventLogDetails));
    }
}