namespace WorkflowEngine.Core;

public class ErrorState : StateDefinition
{
    public ErrorState()
    {
        Name = "Error";
        Webhook = null;
        TriggerWebhookOnExternalEvent = false;
        IsIdle = true;
        Transitions = [];
    }
}