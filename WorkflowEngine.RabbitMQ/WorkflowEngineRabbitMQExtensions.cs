﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Core;

namespace WorkflowEngine.RabbitMQ;

public static class WorkflowEngineRabbitMQExtensions
{
    
    public static WorkflowEngineBuilder UseRabbitMQ(this WorkflowEngineBuilder builder, string hostName,string queueName)
    {
        builder.Services.AddHostedService<ConsumeRabbitMQHostedService>(provider => 
            new ConsumeRabbitMQHostedService(provider, hostName, queueName, provider.GetService<ILogger<ConsumeRabbitMQHostedService>>()));
        builder.Services.AddSingleton<PublishRabbitMQHostedService>(provider => 
            new PublishRabbitMQHostedService(hostName, queueName, provider.GetService<ILogger<PublishRabbitMQHostedService>>()));
        builder.Services.AddSingleton<IHostedService>(p => p.GetRequiredService<PublishRabbitMQHostedService>());
        builder.Services.AddSingleton<IWorkflowEventQueuePublisher, RmqEventQueuePublisher>();
        return builder;
    }
}