using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WorkflowEngine.Core;

public static class WorkflowEngineExtensions
{
    public static WorkflowEngineBuilder AddWorkflowEngine(this IServiceCollection services,
        Action<WorkflowEngineOptions>? configureOptions = null,
        Action<VariableUrlMappings>? configureMappings = null,
        Action<WorkflowActionRegistry>? configureCustomActions = null)
    {
        
        var registry = new WorkflowActionRegistry();
        configureCustomActions?.Invoke(registry);
        services.AddScoped<WorkflowActionRegistry>(provider =>
        {
            
            registry.Register("Webhook", parameters => new WebhookAction(parameters["url"].ToString()));
            
            registry.Register("Custom", parameters => new CustomBehaviorAction( "Custom",
                async (instance, services) =>
                {
                    Console.WriteLine($"Custom action executed for workflow {instance.Id}. Parameters: {JsonSerializer.Serialize(parameters)}");
                    await Task.CompletedTask;
                }));
            return registry;
        });
        // Ensure HttpClient is registered
        if (services.All(sd => sd.ServiceType != typeof(IHttpClientFactory)))
        {
            services.AddHttpClient();
        }
        
        var options = new WorkflowEngineOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);
        
        
        // Default implementations
        services.TryAddSingleton<IWorkflowRepository, InMemoryWorkflowRepository>();
        services.TryAddSingleton<IEventRepository, InMemoryEventRepository>();
        services.TryAddSingleton<IWorkflowEventQueuePublisher, InMemoryWorkflowEventQueue>();
        services.TryAddScoped<IAssignmentResolver, DefaultAssignmentResolver>();
        services.TryAddScoped<IEventLogger, ConsoleEventLogger>();
        services.TryAddScoped<IDataProvider, DefaultDataProvider>();
        
        var variableMappings = new VariableUrlMappings();
        configureMappings?.Invoke(variableMappings);
        services.AddSingleton(variableMappings);
        
        services.AddScoped<IWebhookHandler, WebhookHandler>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        
        // Register the facade
        services.AddScoped<WorkflowEngineFacade>();
        
        
        return new WorkflowEngineBuilder(services);
    }
}