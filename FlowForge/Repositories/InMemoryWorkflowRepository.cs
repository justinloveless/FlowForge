using System.Data.SqlTypes;

namespace FlowForge;

public class InMemoryWorkflowRepository : IWorkflowRepository
{
    private readonly Dictionary<WorkflowDefinitionId, WorkflowDefinition> _workflowDefinitions = new();
    private readonly Dictionary<WorkflowInstanceId, WorkflowInstance> _workflowInstances = new();
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        if (Guid.Empty.Equals(workflow.Id))
            throw new ArgumentNullException(nameof(workflow), "Workflow Id cannot be empty.");
        
        if (!_workflowDefinitions.TryAdd(workflow.Id, workflow))
            throw new InvalidOperationException($"A workflow with the Id '{workflow.Name}' is already registered.");

        return Task.CompletedTask;
    }

    public Task<WorkflowInstance> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData)
    {
        if (!_workflowDefinitions.TryGetValue(workflowId, out var workflowDefinition))
            throw new InvalidOperationException($"No workflow found with the Id '{workflowId}'.");

        var workflowInstance = new WorkflowInstance
        {
            WorkflowName = workflowDefinition.Name,
            DefinitionId = workflowDefinition.Id,
            ActiveStates = [workflowDefinition.InitialState],
            WorkflowData = initialData
        };
        
        _workflowInstances[workflowInstance.Id] = workflowInstance;
        return Task.FromResult(workflowInstance);
    }

    public Task<WorkflowInstance> GetWorkflowInstanceAsync(WorkflowInstanceId instanceId)
    {
        _workflowInstances.TryGetValue(instanceId, out var workflowInstance);
        return Task.FromResult(workflowInstance);
    }

    public Task<IEnumerable<WorkflowDefinition>> GetEventDrivenWorkflowDefinitionsAsync(string eventName)
    {
        return Task.FromResult(
            _workflowDefinitions
            .Where(d => d.Value.IsEventDriven &&
                    d.Value.States.FirstOrDefault(s => s.Name == d.Value.InitialState).Transitions
                            .Any(t => t.Condition.Contains(eventName)))
            .Select(d => d.Value));
    }

    public Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowInstanceId instanceId)
    {
        _workflowInstances.TryGetValue(instanceId, out var workflowInstance);
        _workflowDefinitions.TryGetValue(workflowInstance.DefinitionId, out var workflowDefinition);
        return Task.FromResult(workflowDefinition);
    }
    public Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowDefinitionId definitionId)
    {
        _workflowDefinitions.TryGetValue(definitionId, out var workflowDefinition);
        return Task.FromResult(workflowDefinition);
    }

    public Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(string name)
    {
        var workflowDefinitions = _workflowDefinitions
            .Where(x => x.Value.Name == name)
            .Select(x => x.Value).ToList();
        return Task.FromResult(workflowDefinitions);
    }

    public Task UpdateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        if (!_workflowInstances.ContainsKey(instance.Id))
            throw new InvalidOperationException($"No workflow instance found with the ID '{instance.Id}'.");

        _workflowInstances[instance.Id] = instance;
        return Task.CompletedTask;
    }

    public Task UpdateWorkflowDefinitionAsync(WorkflowDefinition workflow)
    {
        if (!_workflowDefinitions.ContainsKey(workflow.Id))
            throw new InvalidOperationException($"No workflow definition found with the ID '{workflow.Id}'.");
        
        if (workflow.States.All(s => s.Name != "Error"))
            workflow.States.Add(new ErrorState());
        _workflowDefinitions[workflow.Id] = workflow;
        return Task.CompletedTask;
    }
}