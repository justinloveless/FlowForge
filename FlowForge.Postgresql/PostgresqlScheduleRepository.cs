using Microsoft.EntityFrameworkCore;

namespace FlowForge.Postgresql;

public class PostgresqlScheduleRepository: ISchedulingRepository
{
    private readonly WorkflowDbContext _dbContext;

    public PostgresqlScheduleRepository(WorkflowDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddScheduledEvent(ScheduleEvent scheduleEvent)
    {
        scheduleEvent.ResumeTime = DateTime.SpecifyKind(scheduleEvent.ResumeTime, DateTimeKind.Utc);
        _dbContext.ScheduleEvents.Add(scheduleEvent);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveScheduledEvent(ScheduleEvent scheduleEvent)
    {
        _dbContext.ScheduleEvents.Remove(scheduleEvent);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ScheduleEvent>> GetScheduledEvents()
    {
        return await _dbContext.ScheduleEvents.ToListAsync();
    }
}