using System.Data.SqlTypes;

namespace WorkflowEngine.Core;

public class InMemoryWorkflowRepository : IWorkflowRepository
{
    private readonly Dictionary<Guid, WorkflowDefinition> _workflowDefinitions = new();
    private readonly Dictionary<Guid, WorkflowInstance> _workflowInstances = new();
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        if (Guid.Empty.Equals(workflow.Id))
            throw new ArgumentNullException(nameof(workflow.Id), "Workflow Id cannot be empty.");
        
        if (!_workflowDefinitions.TryAdd(workflow.Id, workflow))
            throw new InvalidOperationException($"A workflow with the Id '{workflow.Name}' is already registered.");

        return Task.CompletedTask;
    }

    public Task<WorkflowInstance> StartWorkflowAsync(Guid workflowId, Dictionary<string, object> initialData)
    {
        if (!_workflowDefinitions.TryGetValue(workflowId, out var workflowDefinition))
            throw new InvalidOperationException($"No workflow found with the Id '{workflowId}'.");

        var workflowInstance = new WorkflowInstance
        {
            WorkflowName = workflowDefinition.Name,
            DefinitionId = workflowDefinition.Id,
            CurrentState = workflowDefinition.InitialState,
            StateData = initialData,
        };
        
        _workflowInstances[workflowInstance.Id] = workflowInstance;
        return Task.FromResult(workflowInstance);
    }

    public Task<WorkflowInstance> GetWorkflowInstanceAsync(Guid instanceId)
    {
        _workflowInstances.TryGetValue(instanceId, out var workflowInstance);
        return Task.FromResult(workflowInstance);
    }

    public Task<WorkflowDefinition> GetWorkflowDefinitionAsync(Guid instanceId)
    {
        _workflowInstances.TryGetValue(instanceId, out var workflowInstance);
        _workflowDefinitions.TryGetValue(workflowInstance.DefinitionId, out var workflowDefinition);
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
}