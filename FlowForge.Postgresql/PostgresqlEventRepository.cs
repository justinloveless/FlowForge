using Microsoft.EntityFrameworkCore;
using FlowForge;

namespace FlowForge.Postgresql;

public class PostgresqlEventRepository : IEventRepository
{
    private readonly WorkflowDbContext _dbContext;

    public PostgresqlEventRepository(WorkflowDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddEventAsync(WorkflowEvent workflowEvent)
    {
        _dbContext.WorkflowEvents.Add(workflowEvent);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<WorkflowEvent>> GetEventsAsync(WorkflowInstanceId workflowInstanceId, string? eventType = null)
    {
        var query = _dbContext.WorkflowEvents
            .Where(e => e.WorkflowInstanceId.Equals(workflowInstanceId));

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        return await query.ToListAsync();
    }
}