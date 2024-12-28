using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class WebhookAction : IWorkflowAction
{

    private const string _type = "Webhook";
    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        var webhookHandler = serviceProvider.GetRequiredService<IWebhookHandler>();
        
        var url = parameters["url"].ToString();
        if (string.IsNullOrEmpty(url)) return;
        
        var headersString = parameters.TryGetValue("headers", out var parameter) ? parameter.ToString() : "{}";
        var headers = JsonSerializer.Deserialize<Dictionary<string, object>>(headersString);

        var updatedStateData = await webhookHandler.CallWebhookAsync(url, headers, instance);
        instance.StateData = updatedStateData;
        var eventLogDetails =
            $"State: {instance.CurrentState}, Webhook: {url}, StateData: {JsonSerializer.Serialize(updatedStateData)}";
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