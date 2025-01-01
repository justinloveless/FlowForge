using System.Text.Json;

namespace FlowForge.Demo;

public static class CustomActions
{
    public static readonly CustomBehaviorAction MyCustomAction = new("MyCustomAction",
        (instance, parameters, services) =>
        {
            Console.WriteLine($"MyBehavior executed for workflow {instance.Id}. Parameters: {JsonSerializer.Serialize(parameters)}");
            return Task.CompletedTask;
        });
}