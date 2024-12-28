using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Core;

namespace WorkflowEngine.Postgresql;

public static class WorkflowEnginePostgresqlExtensions
{
    public static WorkflowEngineBuilder UsePostgresql(this WorkflowEngineBuilder builder, string connectionString)
    {
        builder.Services.AddDbContext<WorkflowDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        builder.UseWorkflowRepository<PostgresqlWorkflowRepository>();
        builder.UseEventRepository<PostgresqlEventRepository>();
        
        builder.Services.AddHostedService<MigrationHostedService>();
        return builder;
    }
}