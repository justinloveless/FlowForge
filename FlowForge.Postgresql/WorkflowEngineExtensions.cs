using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowForge.Postgresql;

public static class WorkflowEngineExtensions
{
    public static WorkflowEngineBuilder UsePostgresql(this WorkflowEngineBuilder builder, string connectionString)
    {
        builder.Services.AddDbContext<WorkflowDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        builder.UseWorkflowRepository<PostgresqlWorkflowRepository>();
        builder.UseEventRepository<PostgresqlEventRepository>();
        builder.UseSchedulingRepository<PostgresqlScheduleRepository>();
        
        builder.Services.AddHostedService<MigrationHostedService>();
        return builder;
    }
}