namespace FlowForge;

public class StateBuilder(string name, int? stateIndex = null)
{
    private readonly StateDefinition _state = new()
    {
        Name = name,
        OnEnterActions = [],
        OnExitActions = [],
        Transitions = []
    };
    private readonly int _currentStateIndex = stateIndex ?? 0;
    
    public List<TransitionDefinition> Transitions => _state.Transitions;


    public StateBuilder SetIdle(bool isIdle)
    {
        _state.IsIdle = isIdle;
        return this;
    }

    public StateBuilder AddOnEnterAction(WorkflowAction action)
    {
        _state.OnEnterActions.Add(action);
        return this;
    }

    public StateBuilder OnEnter(WorkflowAction action)
    {
        return AddOnEnterAction(action);
    }

    public StateBuilder AddOnExitAction(WorkflowAction action)
    {
        _state.OnExitActions.Add(action);
        return this;
    }

    public StateBuilder OnExit(WorkflowAction action)
    {
        return AddOnExitAction(action);
    }

    public StateBuilder AddTransition(string condition, string? nextState = null, int? index = null)
    {
        _state.Transitions.Add(new IndexedTransitionDefinition
        {
            Condition = condition,
            NextState = nextState ?? string.Empty,
            Index = nextState is null ? index : null
        });
        return this;
    }

    public StateBuilder Transition(string condition, string? nextState = null)
    {
        return AddTransition(condition, nextState, _currentStateIndex);
    }

    public StateBuilder AddAssignmentUser(string userId)
    {
        _state.Assignments.Users.Add(userId);
        return this;
    }

    public StateBuilder AssignUser(string userId) => AddAssignmentUser(userId);

    public StateBuilder AddAssignmentGroup(string groupName)
    {
        _state.Assignments.Groups.Add(groupName);
        return this;
    }
    
    public StateBuilder AssignGroup(string groupName) => AddAssignmentGroup(groupName);

    public StateDefinition Build()
    {
        return _state;
    }
}