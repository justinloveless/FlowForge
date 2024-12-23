using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public static  class WorkflowEngineServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowEngine(this IServiceCollection services,
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
        
        // Register default implementations
        services.AddSingleton<IWorkflowRepository, InMemoryWorkflowRepository>();
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
        services.AddSingleton<IWorkflowEventQueue, InMemoryWorkflowEventQueue>();
        services.AddTransient<IAssignmentResolver, DefaultAssignmentResolver>();
        services.AddSingleton<IEventLogger, ConsoleEventLogger>();
        
        services.AddSingleton<IWebhookHandler, WebhookHandler>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
        
        
        
        var variableMappings = new VariableUrlMappings();
        configureMappings?.Invoke(variableMappings);
        services.AddSingleton(variableMappings);


        services.AddSingleton<IDataProvider, DefaultDataProvider>();
        
        
        // Register the facade
        services.AddSingleton<WorkflowEngineFacade>();
        
        return services;
    }
}