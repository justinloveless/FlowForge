using System.Text.Json;

namespace FlowForge;

public class WorkflowDefinitionBuilder(string name, string? initialState = null)
{

    private readonly WorkflowDefinition _workflowDefinition = new()
    {
        Name = name,
        States = [],
        InitialState = initialState
    };
    private StateBuilder? _currentStateBuilder;
    private int _stateNameCounter = 0;
    private int _currentStateIndex = 0;
    private readonly Dictionary<int, string> _generatedStateNames = new();

    private string GenerateStateName()
    {
        var stateName = $"State{_currentStateIndex}";
        _generatedStateNames.Add(_currentStateIndex, stateName);
        return stateName;
    }

    public WorkflowDefinitionBuilder Start(Action<StateBuilder>? configure = null)
    {
        _currentStateBuilder = new StateBuilder("Start", _currentStateIndex);
        _generatedStateNames.Add(_currentStateIndex, "Start");
        _currentStateIndex++;
        _workflowDefinition.InitialState = "Start";
        configure?.Invoke(_currentStateBuilder);
        _currentStateBuilder.Transition("true", index: _currentStateIndex);
        return this;
    }

    public WorkflowDefinition End(Action<StateBuilder>? configure = null)
    {
        AddPreviousState();

        var endState = new StateBuilder("End", _currentStateIndex);
        _generatedStateNames.Add(_currentStateIndex, "End");
        _currentStateIndex++;
        configure?.Invoke(_currentStateBuilder);
        _currentStateBuilder = endState;
        return Build();
    }

    public WorkflowDefinitionBuilder Delay(TimeSpan delay, Action<StateBuilder>? configure = null)
    {
        AddPreviousState();
        var action = new TimerAction(delay);
        AddTimerAction(action);
        configure?.Invoke(_currentStateBuilder);
        return this;
    }

    public WorkflowDefinitionBuilder Schedule(DateTime scheduledTime, Action<StateBuilder>? configure = null)
    {
        AddPreviousState();
        var action = new TimerAction(scheduledTime);
        AddTimerAction(action);
        configure?.Invoke(_currentStateBuilder);
        return this;
    }


    public WorkflowDefinitionBuilder ActionableStep(string name, Action<StateBuilder> configure)
    {
        AddPreviousState();
        
        _generatedStateNames.Add(_currentStateIndex, name);
        _currentStateIndex++;
        var stateBuilder = new StateBuilder(name, _currentStateIndex).SetIdle(true);
        configure(stateBuilder);
        
        _currentStateBuilder = stateBuilder;
        return this;
    }


    public WorkflowDefinition Build()
    {
        AddPreviousState();
        
        // Add validation here (e.g., ensure transitions are valid, initialState exists, etc.)
        if (_workflowDefinition.States.All(s => s.Name != _workflowDefinition.InitialState))
        {
            throw new InvalidOperationException("Initial state must be defined in the states.");
        }
        
        _workflowDefinition.States.ForEach(s => s.Transitions.ForEach(t =>
        {
            var generatedName = _generatedStateNames.FirstOrDefault(g => g.Key == ((IndexedTransitionDefinition)t).Index).Value;
            if (generatedName != null)
                t.NextState = generatedName;
        }));
        
        if (_workflowDefinition.States.All(s => s.Transitions.All( t => t.NextState != "End")))
        {
            throw new InvalidOperationException("No transitions go to End state. End state must be reachable");
        }

        return _workflowDefinition;
    }
    
    public WorkflowDefinitionBuilder AddState(string stateName, Action<StateBuilder> configure)
    {
        var stateBuilder = new StateBuilder(stateName);
        configure(stateBuilder);
        _workflowDefinition.States.Add(stateBuilder.Build());
        return this;
    }

    private void AddPreviousState()
    {
        if (_currentStateBuilder != null)
        {
            AddState(_currentStateBuilder.Build());
        }
    }

    private void AddTimerAction(TimerAction action)
    {
        var stateBuilder = new StateBuilder(GenerateStateName(), _currentStateIndex).SetIdle(true);
        _currentStateIndex++;
        _currentStateBuilder = stateBuilder;
        _currentStateBuilder.AddOnEnterAction(action);
        _currentStateBuilder.Transition("event == \"Resume\"", index: _currentStateIndex);
        _currentStateBuilder.AssignUser("system");
    }
    private void AddState(StateDefinition state)
    {
        _workflowDefinition.States.Add(state);
    }

}