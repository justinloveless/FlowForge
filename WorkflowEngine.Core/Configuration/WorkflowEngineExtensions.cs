using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WorkflowEngine.Core;

public static class WorkflowEngineExtensions
{
    public static WorkflowEngineBuilder AddWorkflowEngine(this IServiceCollection services,
        Action<WorkflowEngineOptions>? configureOptions = null,
    Action<VariableUrlMappings>? configureMappings = null)
    {
        
        // Ensure HttpClient is registered
        if (services.All(sd => sd.ServiceType != typeof(IHttpClientFactory)))
        {
            services.AddHttpClient();
        }
        
        var options = new WorkflowEngineOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);
        
        
        // Default implementations
        services.TryAddScoped<IWorkflowRepository, InMemoryWorkflowRepository>();
        services.TryAddScoped<IEventRepository, InMemoryEventRepository>();
        services.TryAddScoped<IWorkflowEventQueue, InMemoryWorkflowEventQueue>();
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