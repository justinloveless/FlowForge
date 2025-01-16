namespace FlowForge;

public class WebhookRegistration
{
    public WebhookRegistrationId Id { get; } = new (Guid.NewGuid());
    public WorkflowDefinitionId WorkflowDefinitionId { get; set; }
    public string EventName { get; set; }
    
}

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct WebhookRegistrationId;