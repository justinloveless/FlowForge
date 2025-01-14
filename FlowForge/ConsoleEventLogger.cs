namespace FlowForge;

internal class ConsoleEventLogger: IEventLogger
{
    private readonly IEventRepository _eventRepository;

    public ConsoleEventLogger(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }
    public async Task<WorkflowEventId> LogEventAsync(string eventType, WorkflowInstanceId? instanceId, WorkflowDefinitionId? definitionId, 
        string details, string? sourceState = null, List<string>? activeStates = null)
    {
        Console.WriteLine($"[{DateTime.UtcNow}] Event: {eventType}, Instance: {instanceId}, Details: {details}");
        activeStates ??= [];


        var workflowEvent = new WorkflowEvent(eventType, instanceId, definitionId, details, sourceState, activeStates);
        await _eventRepository.AddEventAsync(workflowEvent);
        return workflowEvent.Id;
    }
    
}