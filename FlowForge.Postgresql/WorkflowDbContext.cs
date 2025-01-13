using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FlowForge.Postgresql;

public class WorkflowDbContext : DbContext
{
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowEvent> WorkflowEvents { get; set; }
    public DbSet<ScheduleEvent> ScheduleEvents { get; set; }

    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowDefinition>().ToTable("WorkflowDefinitions");
        modelBuilder.Entity<WorkflowInstance>().ToTable("WorkflowInstances");
        modelBuilder.Entity<WorkflowEvent>().ToTable("WorkflowEvents");
        modelBuilder.Entity<ScheduleEvent>().ToTable("ScheduleEvents");

        modelBuilder.Entity<WorkflowDefinition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasConversion(
                        id => id.Value,      // Convert WorkflowDefinitionId to Guid
                        value => new WorkflowDefinitionId(value) // Convert Guid to WorkflowDefinitionId
                    );
                entity.Property(e => e.IsEventDriven).IsRequired().HasDefaultValue(false);
                entity.OwnsMany(e => e.States, state =>
                {
                    state.OwnsOne(e => e.Assignments, ar =>
                    {
                        ar.Property(a => a.Users)
                            .HasConversion(
                                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                            )
                            .HasColumnType("jsonb")
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1.SequenceEqual(c2),  // Compare collections for equality
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Compute hash code
                                c => c.ToList()));
                        ar.Property(a => a.Groups)
                            .HasConversion(
                                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                            )
                            .HasColumnType("jsonb")
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1.SequenceEqual(c2),  // Compare collections for equality
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Compute hash code
                                c => c.ToList()));
                    });

                    state.OwnsMany(e => e.Transitions, t =>
                    {
                        t.Property(td => td.Condition).IsRequired();
                        t.Property(td => td.NextState).IsRequired();
                    });
                    
                    state
                        .Property(s => s.OnEnterActions)
                        .IsRequired(false)
                        .HasConversion(
                            actions => JsonSerializer.Serialize(actions, new JsonSerializerOptions { WriteIndented = false }),
                            json => string.IsNullOrEmpty(json)
                                ? new List<WorkflowAction>()
                                : JsonSerializer.Deserialize<List<WorkflowAction>>(json, (JsonSerializerOptions)null))
                        .HasColumnType("jsonb");

                    state
                        .Property(s => s.OnExitActions)
                        .IsRequired(false)
                        .HasConversion(
                            actions => JsonSerializer.Serialize(actions, new JsonSerializerOptions { WriteIndented = false }),
                            json => string.IsNullOrEmpty(json)
                                ? new List<WorkflowAction>()
                                : JsonSerializer.Deserialize<List<WorkflowAction>>(json, (JsonSerializerOptions)null))
                        .HasColumnType("jsonb");

                }
                );
            });

        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasConversion(
                    id => id.Value, // Convert WorkflowInstanceId to Guid
                    value => new WorkflowInstanceId(value) // Convert Guid to WorkflowInstanceId
                );
            entity.Property(e => e.DefinitionId)
                .HasConversion(
                    id => id.Value,      // Convert WorkflowDefinitionId to Guid
                    value => new WorkflowDefinitionId(value) // Convert Guid to WorkflowDefinitionId
                );
            entity.Property(e => e.StateData)
                .HasColumnType("jsonb") // Use 'jsonb' for PostgreSQL
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),  // Convert Dictionary to JSON
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null) // Convert JSON to Dictionary
                )
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>>(
                    (d1, d2) => d1.SequenceEqual(d2), // Equality comparison
                    d => d.Aggregate(0, (hash, kvp) => HashCode.Combine(hash, kvp.Key.GetHashCode(), kvp.Value.GetHashCode())), // Hash code
                    d => d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));;
            entity.Property(e => e.WorkflowData)
                .HasColumnType("jsonb") // Use 'jsonb' for PostgreSQL
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),  // Convert Dictionary to JSON
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null) // Convert JSON to Dictionary
                )
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>>(
                    (d1, d2) => d1.SequenceEqual(d2), // Equality comparison
                    d => d.Aggregate(0, (hash, kvp) => HashCode.Combine(hash, kvp.Key.GetHashCode(), kvp.Value.GetHashCode())), // Hash code
                    d => d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        });

        modelBuilder.Entity<WorkflowEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => new WorkflowEventId(value)
                );
            entity.Property(e => e.WorkflowInstanceId)
                .HasConversion(
                    id => id.Value,      // Convert WorkflowInstanceId to Guid
                    value => new WorkflowInstanceId(value) // Convert Guid to WorkflowInstanceId
                );
            entity.Property(e => e.WorkflowDefinitionId)
                .HasConversion(
                    id => id.Value,      // Convert WorkflowInstanceId to Guid
                    value => new WorkflowDefinitionId(value) // Convert Guid to WorkflowInstanceId
                );
        });

        modelBuilder.Entity<ScheduleEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ScheduleEventId(value)
                );
            entity.Property(e => e.InstanceId)
                .HasConversion(
                    id => id.Value,      
                    value => new WorkflowInstanceId(value)
                );
            
        });



    }
}