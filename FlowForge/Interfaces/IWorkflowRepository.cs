namespace FlowForge;

public interface IWorkflowRepository
{
    Task RegisterWorkflowAsync(WorkflowDefinition workflow);
    Task<WorkflowInstance> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData);
    Task<WorkflowInstance> GetWorkflowInstanceAsync(WorkflowInstanceId instanceId);
    Task<IEnumerable<WorkflowDefinition>> GetEventDrivenWorkflowDefinitionsAsync(string eventName);
    Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowInstanceId instanceId);
    Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowDefinitionId instanceId);
    Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(string name);
    Task UpdateWorkflowInstanceAsync(WorkflowInstance instance);
    Task UpdateWorkflowDefinitionAsync(WorkflowDefinition workflow);
}