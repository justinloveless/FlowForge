namespace FlowForge;

internal class ConsoleEventLogger: IEventLogger
{
    public Task LogEventAsync(string eventType, WorkflowInstanceId? instanceId, string details)
    {
        Console.WriteLine($"[{DateTime.UtcNow}] Event: {eventType}, Instance: {instanceId}, Details: {details}");
        return Task.CompletedTask;
    }
    
}