namespace FlowForge;

public class FlowForge(
    IWorkflowEngine workflowEngine,
    IWorkflowRepository workflowRepository,
    IEventRepository eventRepository,
    IAssignmentResolver assignmentResolver)
{
    // Workflow engine methods
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow) => workflowEngine.RegisterWorkflowAsync(workflow);

    public Task<WorkflowInstanceId> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData) =>
        workflowEngine.StartWorkflowAsync(workflowId, initialData);

    public Task TriggerEventAsync(WorkflowInstanceId instanceId, string eventName, Dictionary<string, object> eventData, string actorId) =>
        workflowEngine.TriggerEventAsync(instanceId, eventName, eventData, actorId);

    // Repository and event-specific methods
    public Task<IEnumerable<WorkflowEvent>> GetWorkflowEventsAsync(WorkflowInstanceId instanceId, string? eventType = null) =>
        eventRepository.GetEventsAsync(instanceId, eventType);

    public async Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(WorkflowDefinitionId workflowId) =>
        await workflowRepository.GetWorkflowDefinitionAsync(workflowId);
    public async Task<IEnumerable<WorkflowDefinition>> GetWorkflowDefinitionsByNameAsync(string name) => 
        await workflowRepository.GetWorkflowDefinitionsAsync(name);

    public async Task<WorkflowInstance> GetWorkflowInstanceAsync(WorkflowInstanceId workflowInstanceId) =>
        await workflowRepository.GetWorkflowInstanceAsync(workflowInstanceId);

    public async Task UpdateWorkflowDefinitionAsync(WorkflowDefinition workflowDefinition) =>
        await workflowRepository.UpdateWorkflowDefinitionAsync(workflowDefinition);
    
    public async Task<IEnumerable<string>> GetAssignedActorsAsync(string stateName, WorkflowInstanceId workflowInstanceId) =>
        await assignmentResolver.GetAssignmentsAsync(stateName, workflowInstanceId);
    
    
}