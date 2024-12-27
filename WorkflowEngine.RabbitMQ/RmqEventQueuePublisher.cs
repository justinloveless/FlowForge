using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Core;

namespace WorkflowEngine.RabbitMQ;

public class RmqEventQueuePublisher(PublishRabbitMQHostedService service) : IWorkflowEventQueuePublisher
{
    public async Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        await service.PublishEventAsync(workflowInstanceId, eventName, eventData);
    }

}