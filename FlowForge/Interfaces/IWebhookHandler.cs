namespace FlowForge;

public interface IWebhookHandler
{
    Task<Dictionary<string, object>> CallWebhookAsync(string webhookUrl, Dictionary<string, object> headers, WorkflowInstance instance);
    public Task HandleWebhookAsync(WebhookRegistrationId webhookId, Dictionary<string, object> webhookData);
}