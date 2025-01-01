using WorkflowEngine.Core;

namespace WorkflowEngine.Mermaid;

public static class WorkflowEngineFacadeExtensions
{
    public static async Task<string> ConvertToMermaidAsync(this WorkflowEngineFacade facade, WorkflowDefinition definition, bool showDetails = false) => MermaidGenerator.ConvertToMermaidDiagram(definition, showDetails);
}