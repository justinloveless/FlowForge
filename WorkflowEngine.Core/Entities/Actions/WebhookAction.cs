using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class WebhookAction(string url) : IWorkflowAction
{
    private string _url { get; set; } = url;

    private static string _type => "Webhook";
    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        var webhookHandler = serviceProvider.GetRequiredService<IWebhookHandler>();
        
        if (string.IsNullOrEmpty(_url)) return;
        
        var updatedStateData = await webhookHandler.CallWebhookAsync(_url, instance);
        instance.StateData = updatedStateData;
        var eventLogDetails =
            $"State: {instance.CurrentState}, Webhook: {_url}, StateData: {JsonSerializer.Serialize(updatedStateData)}";
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