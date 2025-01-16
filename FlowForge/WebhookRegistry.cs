namespace FlowForge;

public class WebhookRegistry: IWebhookRegistry
{
    private readonly Dictionary<WebhookRegistrationId, WebhookRegistration> _webhooks = new();

    public async Task<WebhookRegistrationId> RegisterWebhook(WorkflowDefinitionId definitionId, string eventName)
    {
        var webhookRegistration = new WebhookRegistration()
        {
            WorkflowDefinitionId = definitionId,
            EventName = eventName
        };
        _webhooks[webhookRegistration.Id] = webhookRegistration;
        return webhookRegistration.Id;
    }

    public Task<WebhookRegistration?> GetWebhookRegistrationAsync(WebhookRegistrationId webhookid)
    {
        _webhooks.TryGetValue(webhookid, out var registration);
        return Task.FromResult(registration);
    }
}