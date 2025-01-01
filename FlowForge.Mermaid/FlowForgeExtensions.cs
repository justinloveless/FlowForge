namespace FlowForge.Mermaid;

public static class FlowForgeExtensions
{
    public static async Task<string> ConvertToMermaidAsync(this FlowForge facade, WorkflowDefinition definition, bool showDetails = false) => MermaidGenerator.ConvertToMermaidDiagram(definition, showDetails);
}