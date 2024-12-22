namespace WorkflowEngine.Core;

public interface IWorkflowRepository
{
    Task RegisterWorkflowAsync(WorkflowDefinition workflow);
    Task<WorkflowInstance> StartWorkflowAsync(Guid workflowId, Dictionary<string, object> initialData);
    Task<WorkflowInstance> GetWorkflowInstanceAsync(Guid instanceId);
    Task<WorkflowDefinition> GetWorkflowDefinitionAsync(Guid instanceId);
    Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(string name);
    Task UpdateWorkflowInstanceAsync(WorkflowInstance instance);
}