namespace WorkflowEngine.Core;

public class TransitionDefinition
{
    public string Condition { get; set; } = string.Empty;
    public string NextState { get; set; } = string.Empty;
}