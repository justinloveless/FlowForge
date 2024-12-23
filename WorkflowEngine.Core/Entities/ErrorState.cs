namespace WorkflowEngine.Core;

public class ErrorState : StateDefinition
{
    public ErrorState()
    {
        Name = "Error";
        Webhook = string.Empty;
        TriggerWebhookOnExternalEvent = false;
        IsIdle = true;
        Transitions = [];
    }
}