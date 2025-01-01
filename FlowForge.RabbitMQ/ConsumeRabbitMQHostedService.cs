using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using FlowForge;

namespace FlowForge.RabbitMQ;

public class ConsumeRabbitMQHostedService(IServiceProvider provider, string hostName, string queueName, ILogger<ConsumeRabbitMQHostedService> logger) 
    : RabbitMQHostedService(hostName, queueName, logger)
{

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"RabbitMQ Rabbit MQ Queue: {queueName}");
        await EnsureInitRabbitMq(stoppingToken);
        stoppingToken.ThrowIfCancellationRequested();
        logger.LogInformation("Starting RabbitMQ Consumer");
        
        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            await HandleMessage(content);
            await Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
        };
        
        consumer.ShutdownAsync += OnConsumerShutdown;
        consumer.RegisteredAsync += OnConsumerRegistered;
        consumer.UnregisteredAsync += OnConsumerUnregistered;
        await Channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }

    private async Task HandleMessage(string message)
    {
        using var scope = provider.CreateScope();
        var workflowEngine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();
        logger.LogInformation("HandleMessage: {Message}", message);
        logger.LogInformation("Message received on queue: {QueueName}", queueName);

        var receivedEvent = JsonSerializer.Deserialize<EventMessage>(message);
        if (receivedEvent != null)
        {
            logger.LogInformation("Processing event: {ReceivedEventEventName}", receivedEvent.EventName);
            await workflowEngine.HandleEventAsync( receivedEvent.WorkflowInstanceId, receivedEvent.EventName, receivedEvent.EventData);
        }
    }
}