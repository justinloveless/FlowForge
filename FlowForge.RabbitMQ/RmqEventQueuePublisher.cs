using Microsoft.Extensions.DependencyInjection;
using FlowForge;

namespace FlowForge.RabbitMQ;

public class RmqEventQueuePublisher(PublishRabbitMQHostedService service) : IWorkflowEventQueuePublisher
{
    public async Task PublishEventAsync(WorkflowInstanceId?  workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        await service.PublishEventAsync(workflowInstanceId, eventName, eventData);
    }

}