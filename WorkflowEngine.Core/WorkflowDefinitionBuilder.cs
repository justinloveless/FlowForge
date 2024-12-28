using System.Text.Json;

namespace WorkflowEngine.Core;

public class WorkflowDefinitionBuilder
{

    private readonly WorkflowDefinition _workflowDefinition;

    public WorkflowDefinitionBuilder(string name, string initialState)
    {
        _workflowDefinition = new WorkflowDefinition
        {
            Name = name,
            InitialState = initialState,
            States = []
        };
    }

    public WorkflowDefinitionBuilder AddState(string stateName, Action<StateBuilder> configure)
    {
        var stateBuilder = new StateBuilder(stateName);
        configure(stateBuilder);
        _workflowDefinition.States.Add(stateBuilder.Build());
        return this;
    }
    
    
    public WorkflowDefinition Build()
    {
        // Add validation here (e.g., ensure transitions are valid, initialState exists, etc.)
        if (_workflowDefinition.States.All(s => s.Name != _workflowDefinition.InitialState))
        {
            throw new InvalidOperationException("Initial state must be defined in the states.");
        }

        return _workflowDefinition;
    }

    public string BuildJson()
    {
        return JsonSerializer.Serialize(Build(), new JsonSerializerOptions { WriteIndented = true });
    }

}