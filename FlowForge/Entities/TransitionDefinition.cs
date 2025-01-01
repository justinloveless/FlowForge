namespace FlowForge;

public class TransitionDefinition
{
    public string Condition { get; set; } = string.Empty;
    public string NextState { get; set; } = string.Empty;
}

internal class IndexedTransitionDefinition : TransitionDefinition
{
    public int? Index { get; set; }
}