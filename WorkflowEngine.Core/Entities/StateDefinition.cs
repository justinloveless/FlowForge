namespace WorkflowEngine.Core;

public class StateDefinition
{
    public string Name { get; set; } = string.Empty;
    public List<WorkflowAction>? OnEnterActions { get; set; } = [];
    public List<WorkflowAction>? OnExitActions { get; set; } = [];
    public bool IsIdle { get; set; } = false;
    public AssignmentRules Assignments { get; set; } = new();
    public List<TransitionDefinition> Transitions { get; set; } = [];
}