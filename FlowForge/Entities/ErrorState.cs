namespace FlowForge;

public class ErrorState : StateDefinition
{
    public ErrorState()
    {
        Name = "Error";
        IsIdle = true;
        Transitions = [];
        OnEnterActions = [];
        OnExitActions = [];
        Assignments = new AssignmentRules();
    }
}