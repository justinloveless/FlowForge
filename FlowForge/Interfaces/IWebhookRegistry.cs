namespace FlowForge;

public interface IWebhookRegistry
{
    public Task<WebhookRegistrationId> RegisterWebhook(WorkflowDefinitionId definitionId, string eventName);
    public Task<WebhookRegistration?> GetWebhookRegistrationAsync(WebhookRegistrationId webhookid);
}