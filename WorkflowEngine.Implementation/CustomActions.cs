using System.Text.Json;
using WorkflowEngine.Core;

namespace WorkflowEngine.Implementation;

public static class CustomActions
{
    public static readonly CustomBehaviorAction MyCustomAction = new("MyCustomAction",
        (instance, parameters, services) =>
        {
            Console.WriteLine($"MyBehavior executed for workflow {instance.Id}. Parameters: {JsonSerializer.Serialize(parameters)}");
            return Task.CompletedTask;
        });
}