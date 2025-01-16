namespace FlowForge.Postgresql;

public class PostgresqlWebhookRepository(WorkflowDbContext dbContext): IWebhookRegistry
{
    public async Task<WebhookRegistrationId> RegisterWebhook(WorkflowDefinitionId definitionId, string eventName)
    {
        var webhook = new WebhookRegistration
        {
            WorkflowDefinitionId = definitionId,
            EventName = eventName
        };
        dbContext.WebhookRegistrations.Add(webhook);
        await dbContext.SaveChangesAsync();
        return webhook.Id;
    }

    public async Task<WebhookRegistration?> GetWebhookRegistrationAsync(WebhookRegistrationId webhookid)
    {
        return await dbContext.WebhookRegistrations.FindAsync(webhookid)
               ?? throw new InvalidOperationException($"No webhook found with the ID '{webhookid}'.");
    }
}