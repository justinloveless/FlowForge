using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WorkflowEngine.RabbitMQ;

public abstract class RabbitMQHostedService: BackgroundService
{
    protected IConnection Connection;
    protected IChannel Channel;
    private readonly string _hostName;
    protected readonly string QueueName = "default";
    private readonly ILogger<RabbitMQHostedService> _logger;

    public RabbitMQHostedService(string hostName, string queueName, ILogger<RabbitMQHostedService> logger)
    {
        _hostName = hostName;
        QueueName = queueName;
        _logger = logger;
    }
    
    protected async Task EnsureInitRabbitMq(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Connecting to RabbitMQ Server");
        if (Connection != null && Channel != null) return;
        
        var factory = new ConnectionFactory {HostName = _hostName };

        var retryCount = 0;
        const int maxRetries = 5;
        const int initialDelayMs = 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Connection = await factory.CreateConnectionAsync(stoppingToken);
                Channel = await Connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
                await Channel.QueueDeclareAsync(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

                Connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
                _logger.LogInformation("Connection: {Connection}", JsonSerializer.Serialize(Connection));
                _logger.LogInformation("Channel: {Channel}", JsonSerializer.Serialize(Channel));
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, "Failed to connect to RabbitMQ. Retrying in {DelaySeconds} seconds...", retryCount);

                if (retryCount >= maxRetries)
                {
                    _logger.LogCritical("Max retry attempts reached. RabbitMQ service is unavailable.");
                    throw; // Fail the application if retries are exhausted
                }

                // Exponential backoff with a maximum delay
                var delay = Math.Min(initialDelayMs * (int)Math.Pow(2, retryCount), 30000); // Cap delay at 30 seconds
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
    protected async Task OnConsumerUnregistered(object sender, ConsumerEventArgs e)
    {
        Console.WriteLine("OnConsumerUnregistered");
    }

    protected async Task OnConsumerRegistered(object sender, ConsumerEventArgs e)
    {
        Console.WriteLine("OnConsumerRegistered");
    }

    protected async Task OnConsumerShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("OnConsumerShutdown");
    }

    protected async Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("RabbitMQ_ConnectionShutdown");
    }
}