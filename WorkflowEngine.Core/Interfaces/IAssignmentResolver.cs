namespace WorkflowEngine.Core;

public interface IAssignmentResolver
{
    Task<bool> CanActOnStateAsync(string stateName, WorkflowInstanceId workflowInstanceId, string userId);
    Task<IEnumerable<string>> GetAssignmentsAsync(string stateName, WorkflowInstanceId workflowInstanceId);
}