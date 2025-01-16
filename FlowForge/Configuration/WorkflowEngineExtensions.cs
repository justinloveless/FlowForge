using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowForge;

public static class WorkflowEngineExtensions
{
    public static WorkflowEngineBuilder AddFlowForge(this IServiceCollection services,
        Action<WorkflowEngineOptions>? configureOptions = null,
        Action<VariableUrlMappings>? configureMappings = null,
        Action<WorkflowActionRegistry>? configureCustomActions = null)
    {
        
        var registry = new WorkflowActionRegistry();
        configureCustomActions?.Invoke(registry);
        services.AddScoped<WorkflowActionRegistry>(provider =>
        {
            
            registry.Register("Webhook", parameters => new WebhookAction());
            registry.Register("EmitEvent", parameters => new EmitEventAction());
            registry.Register("Timer", parameters => 
                new TimerAction(provider));
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
        services.TryAddSingleton<ISchedulingRepository, InMemoryScheduleRepository>();
        services.TryAddSingleton<IWorkflowEventQueuePublisher, InMemoryWorkflowEventQueue>();
        services.TryAddSingleton<IWebhookRegistry, WebhookRegistry>();
        services.TryAddScoped<IAssignmentResolver, DefaultAssignmentResolver>();
        services.TryAddScoped<IEventLogger, ConsoleEventLogger>();
        services.TryAddScoped<IDataProvider, DefaultDataProvider>();
        services.AddScoped<IConditionEngine, ConditionEngine>();
        
        var variableMappings = new VariableUrlMappings();
        configureMappings?.Invoke(variableMappings);
        services.AddSingleton(variableMappings);
        
        services.AddScoped<IWebhookHandler, WebhookHandler>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddSingleton<SchedulerHostedService>();
        services.AddHostedService<SchedulerHostedService>();
        
        // Register the facade
        services.AddScoped<FlowForge>();
        
        
        return new WorkflowEngineBuilder(services);
    }
}