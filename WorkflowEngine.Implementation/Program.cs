using WorkflowEngine.Core;

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
                mappings.AddMapping("UserState", "https://localhost:44307/webhook/provider/userstate/{userId}");
                mappings.AddMapping("UserAge", "https://localhost:44307/webhook/provider/userage/{userId}");
                mappings.AddMapping("Trainings", "https://localhost:44307/webhook/provider/trainings/{instanceId}");
            }
        );

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