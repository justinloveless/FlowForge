using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class WorkflowEngineBuilder
{
    readonly IServiceCollection _services;

    public IServiceCollection Services => _services;

    public WorkflowEngineBuilder(IServiceCollection services)
    {
        _services = services;
    }
    
    public WorkflowEngineBuilder UseWorkflowRepository<T>() where T : class, IWorkflowRepository
    {
        _services.AddSingleton<IWorkflowRepository, T>();
        return this;
    }
    
    public WorkflowEngineBuilder UseWorkflowRepository<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IWorkflowRepository
    {
        _services.AddSingleton<IWorkflowRepository, T>(implementationFactory);
        return this;
    }

    public WorkflowEngineBuilder UseEventRepository<T>() where T : class, IEventRepository
    {
        _services.AddSingleton<IEventRepository, T>();
        return this;
    }
    public WorkflowEngineBuilder UseEventRepository<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IEventRepository
    {
        _services.AddSingleton<IEventRepository, T>(implementationFactory);
        return this;
    }

    public WorkflowEngineBuilder UseWorkflowEventQueue<T>() where T : class, IWorkflowEventQueue
    {
        _services.AddSingleton<IWorkflowEventQueue, T>();
        return this;
    }
    public WorkflowEngineBuilder UseWorkflowEventQueue<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IWorkflowEventQueue
    {
        _services.AddSingleton<IWorkflowEventQueue, T>(implementationFactory);
        return this;
    }

    public WorkflowEngineBuilder UseAssignmentResolver<T>() where T : class, IAssignmentResolver
    {
        _services.AddSingleton<IAssignmentResolver, T>();
        return this;
    }
    public WorkflowEngineBuilder UseAssignmentResolver<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IAssignmentResolver
    {
        _services.AddSingleton<IAssignmentResolver, T>(implementationFactory);
        return this;
    }

    public WorkflowEngineBuilder UseEventLogger<T>() where T : class, IEventLogger
    {
        _services.AddSingleton<IEventLogger, T>();
        return this;
    }
    public WorkflowEngineBuilder UseEventLogger<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IEventLogger
    {
        _services.AddSingleton<IEventLogger, T>(implementationFactory);
        return this;
    }

    public WorkflowEngineBuilder UseDataProvider<T>() where T : class, IDataProvider
    {
        _services.AddSingleton<IDataProvider, T>();
        return this;
    }
    public WorkflowEngineBuilder UseDataProvider<T>(Func<IServiceProvider, T> implementationFactory) where T : class, IDataProvider
    {
        _services.AddSingleton<IDataProvider, T>(implementationFactory);
        return this;
    }
}