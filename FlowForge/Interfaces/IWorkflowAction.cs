namespace FlowForge;

public interface IWorkflowAction
{
    Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider);
}