using WorkflowEngine.Core;
using WorkflowEngine.Postgresql;

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
            }
        ).UsePostgresql("Host=postgres;Database=workflow;Username=postgres;Password=password");

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