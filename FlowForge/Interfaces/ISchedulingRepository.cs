namespace FlowForge;

public interface ISchedulingRepository
{
    Task AddScheduledEvent(ScheduleEvent scheduleEvent);
    Task RemoveScheduledEvent(ScheduleEvent scheduleEvent);
    Task<IEnumerable<ScheduleEvent>> GetScheduledEvents();
}