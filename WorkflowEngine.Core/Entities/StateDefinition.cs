namespace WorkflowEngine.Core;

public class StateDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Webhook { get; set; } = string.Empty;
    public bool TriggerWebhookOnExternalEvent { get; set; } = true;
    public bool IsIdle { get; set; } = false;
    public List<TransitionDefinition> Transitions { get; set; } = [];
}