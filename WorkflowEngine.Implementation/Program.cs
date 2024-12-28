using System.Text.Json;
using WorkflowEngine.Core;
using WorkflowEngine.Postgresql;
using WorkflowEngine.RabbitMQ;

namespace WorkflowEngine.Implementation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddWorkflowEngine(
            configureOptions: options =>
            {
                options.EnableDetailedLogging = true;
                options.UseInMemoryRepositories = true;
            },
            configureMappings: mappings =>
            {
                mappings.AddMapping("UserState", "http://localhost:8080/webhook/provider/userstate/{userId}");
                mappings.AddMapping("UserAge", "http://localhost:8080/webhook/provider/userage/{userId}");
                mappings.AddMapping("Trainings", "http://localhost:8080/webhook/provider/trainings/{instanceId}");
            },
            configureCustomActions: customActions =>
            {
                customActions.Register( "MyBehavior", parameters => 
                    new CustomBehaviorAction("MyBehavior", 
                        async (instance, services) =>
                    {
                        Console.WriteLine($"MyBehavior executed for workflow {instance.Id}. Parameters: {JsonSerializer.Serialize(parameters)}");
                        await Task.CompletedTask;
                    }));
            }
        )
            .UseAssignmentResolver<CustomAssignmentResolver>()
            .UsePostgresql("Host=postgres;Database=workflow;Username=postgres;Password=password")
            .UseRabbitMQ("rabbitmq", "workflow-queue");

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}