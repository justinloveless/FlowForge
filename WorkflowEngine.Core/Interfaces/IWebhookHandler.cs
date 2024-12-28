namespace WorkflowEngine.Core;

public interface IWebhookHandler
{
    Task<Dictionary<string, object>> CallWebhookAsync(string webhookUrl, Dictionary<string, object> headers, WorkflowInstance instance);
}