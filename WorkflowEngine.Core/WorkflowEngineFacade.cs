namespace WorkflowEngine.Core;

public class WorkflowEngineFacade
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IWorkflowEventQueue _workflowEventQueue;

    public WorkflowEngineFacade(IWorkflowEngine workflowEngine,
        IWorkflowRepository workflowRepository,
        IEventRepository eventRepository,
        IWorkflowEventQueue workflowEventQueue)
    {
        _workflowEngine = workflowEngine;
        _workflowRepository = workflowRepository;
        _eventRepository = eventRepository;
        _workflowEventQueue = workflowEventQueue;
    }
    // Workflow engine methods
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow) => _workflowEngine.RegisterWorkflowAsync(workflow);

    public Task<Guid> StartWorkflowAsync(Guid workflowId, Dictionary<string, object> initialData) =>
        _workflowEngine.StartWorkflowAsync(workflowId, initialData);

    public Task TriggerEventAsync(Guid instanceId, string eventName, Dictionary<string, object> eventData) =>
        _workflowEngine.TriggerEventAsync(instanceId, eventName, eventData);

    // Repository and event-specific methods
    public Task<IEnumerable<WorkflowEvent>> GetWorkflowEventsAsync(Guid instanceId, string? eventType = null) =>
        _eventRepository.GetEventsAsync(instanceId, eventType);

    public async Task<IEnumerable<WorkflowDefinition>> GetWorkflowDefinitionsByNameAsync(string name) => 
        await _workflowRepository.GetWorkflowDefinitionsAsync(name);

    public async Task<WorkflowInstance> GetWorkflowInstanceAsync(Guid workflowInstanceId) =>
        await _workflowRepository.GetWorkflowInstanceAsync(workflowInstanceId);
}