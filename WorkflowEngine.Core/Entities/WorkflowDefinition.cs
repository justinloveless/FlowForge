namespace WorkflowEngine.Core;

public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string InitialState { get; set; } = string.Empty;
    public List<StateDefinition> States { get; set; } = [];
}