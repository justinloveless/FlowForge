namespace WorkflowEngine.Core;

public class StateBuilder
{
    
    private readonly StateDefinition _state;

    public StateBuilder(string name)
    {
        _state = new StateDefinition
        {
            Name = name,
            OnEnterActions = new List<WorkflowAction>(),
            OnExitActions = new List<WorkflowAction>(),
            Transitions = new List<TransitionDefinition>()
        };
    }

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

    public StateBuilder AddOnExitAction(WorkflowAction action)
    {
        _state.OnExitActions.Add(action);
        return this;
    }

    public StateBuilder AddTransition(string condition, string nextState)
    {
        _state.Transitions.Add(new TransitionDefinition
        {
            Condition = condition,
            NextState = nextState
        });
        return this;
    }

    public StateDefinition Build()
    {
        return _state;
    }
}