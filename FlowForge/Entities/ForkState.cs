namespace FlowForge;

public class ForkState : StateDefinition
{
    public string Type { get; set; } = "Fork";
    public List<string> ParallelPaths { get; set; } = [];
    public string JoinState { get; set; }
    
}