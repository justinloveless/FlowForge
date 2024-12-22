namespace WorkflowEngine.Core;

public class WorkflowInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public Dictionary<string, object> StateData { get; set; } = new();
}