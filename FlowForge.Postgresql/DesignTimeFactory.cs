using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowForge.Postgresql;

public class DesignTimeFactory : IDesignTimeDbContextFactory<WorkflowDbContext>
{
    public WorkflowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkflowDbContext>();
        optionsBuilder.UseNpgsql("Host=postgres;Database=workflow;Username=postgres;Password=password");
        return new WorkflowDbContext(optionsBuilder.Options);
    }
}