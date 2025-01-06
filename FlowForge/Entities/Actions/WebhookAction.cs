using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge;

public class WebhookAction : WorkflowAction, IWorkflowAction
{

    public WebhookAction()
    {
        Type = "Webhook";
        Parameters = new Dictionary<string, object>();
    }
    public WebhookAction(string url, Dictionary<string, string>? headers = null)
    {
        Type = "Webhook";
        Parameters = new Dictionary<string, object>
        {
            {"url", url}, 
            {"headers", headers}
        };
    }

    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var eventLogger = serviceProvider.GetRequiredService<IEventLogger>();
        var eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
        var webhookHandler = serviceProvider.GetRequiredService<IWebhookHandler>();
        
        var url = parameters["url"].ToString();
        if (string.IsNullOrEmpty(url)) return;

        var headersExists = parameters.TryGetValue("headers", out var parameter);
        var headersString = headersExists && parameter is not null ? parameter.ToString() : "{}";
        var headers = JsonSerializer.Deserialize<Dictionary<string, object>>(headersString);

        var updatedStateData = await webhookHandler.CallWebhookAsync(url, headers, instance);
        instance.StateData = updatedStateData;
        var eventLogDetails =
            $"State: {instance.CurrentState}, Webhook: {url}, StateData: {JsonSerializer.Serialize(updatedStateData)}";
        await eventLogger.LogEventAsync($"{Type}Executed", instance.Id, eventLogDetails);

        await eventRepository.AddEventAsync(new WorkflowEvent
        {
            WorkflowInstanceId = instance.Id,
            WorkflowDefinitionId = instance.DefinitionId,
            EventType = $"{Type}Executed",
            CurrentState = instance.CurrentState,
            Details = eventLogDetails,
            Timestamp = DateTime.UtcNow
        });
    }
}