using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using WorkflowEngine.Core;

namespace WorkflowEngine.RabbitMQ;

public class PublishRabbitMQHostedService(string hostName, string queueName, ILogger<PublishRabbitMQHostedService> logger) 
    : RabbitMQHostedService(hostName, queueName, logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureInitRabbitMq(stoppingToken);
        stoppingToken.ThrowIfCancellationRequested();
        // we don't need to do anything on execute. We just need the channel to stay open
    }
    
    public async Task PublishEventAsync(string workflowInstanceId, string eventName, Dictionary<string, object> eventData)
    {
        var eventMessage = new EventMessage
        {
            WorkflowInstanceId = workflowInstanceId,
            EventName = eventName,
            EventData = eventData
        };

        var message = JsonSerializer.Serialize(eventMessage);
        var body = Encoding.UTF8.GetBytes(message);

        await Channel.BasicPublishAsync(
            exchange: "",
            routingKey: QueueName,
            body: body);
    }

}