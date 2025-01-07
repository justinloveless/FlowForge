namespace FlowForge;

public class JoinState: StateDefinition
{
    public string Type { get; set; } = "Join";
    public List<string> WaitForEvents { get; set; } = [];
    public WorkflowEventId ForkEventId { get; init; } = new(Guid.Empty);
}