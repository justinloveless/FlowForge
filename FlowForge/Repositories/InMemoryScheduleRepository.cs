namespace FlowForge;

public class InMemoryScheduleRepository: ISchedulingRepository
{
    private readonly List<ScheduleEvent> _scheduleEvents = [];
    public Task AddScheduledEvent(ScheduleEvent scheduleEvent)
    {
        _scheduleEvents.Add(scheduleEvent);
        return Task.CompletedTask;
    }

    public Task RemoveScheduledEvent(ScheduleEvent scheduleEvent)
    {
        _scheduleEvents.Remove(scheduleEvent);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ScheduleEvent>> GetScheduledEvents()
    {
        return Task.FromResult(_scheduleEvents.AsEnumerable());
    }
}