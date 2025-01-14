namespace FlowForge;

public interface IConditionEngine
{
    internal Task<bool> EvaluateCondition(string condition, WorkflowInstance? instance, string actingState, string? eventName);
}