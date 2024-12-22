namespace WorkflowEngine.Core;

public class ConsoleEventLogger: IEventLogger
{
    public Task LogEventAsync(string eventType, Guid? instanceId, string details)
    {
        Console.WriteLine($"[{DateTime.UtcNow}] Event: {eventType}, Instance: {instanceId}, Details: {details}");
        return Task.CompletedTask;
    }
    
}