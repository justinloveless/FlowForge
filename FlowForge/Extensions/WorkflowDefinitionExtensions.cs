using System.Text.Json;

namespace FlowForge;

public static class WorkflowDefinitionExtensions
{
    public static string ToJson(this WorkflowDefinition workflowDefinition)
    {
        return JsonSerializer.Serialize(workflowDefinition, new JsonSerializerOptions { WriteIndented = true });
    }
}