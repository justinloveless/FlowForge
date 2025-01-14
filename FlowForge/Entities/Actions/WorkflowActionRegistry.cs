namespace FlowForge;

public class WorkflowActionRegistry
{
    private readonly Dictionary<string, Func<IDictionary<string, object>, IWorkflowAction>> _actions = new();

    public void Register<TAction>(string type, Func<IDictionary<string, object>, TAction> factory)
    where TAction : IWorkflowAction
    {
        _actions[type] = parameters => factory(parameters);
    }
    
    public List<string> RegisteredActions => _actions.Keys.ToList(); 

    internal IWorkflowAction Create(string type, IDictionary<string, object> parameters)
    {
        if (!_actions.TryGetValue(type, out var factory))
            throw new InvalidOperationException($"No action registered for {type}");
        
        return factory(parameters);
    }
}