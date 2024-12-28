using WorkflowEngine.Core;

namespace WorkflowEngine.Implementation;

public class CustomAssignmentResolver(IWorkflowRepository workflowRepository) : IAssignmentResolver
{
    public async Task<bool> CanActOnStateAsync(string stateName, WorkflowInstanceId workflowInstanceId, string userId)
    {
        Console.WriteLine($"CanActOnStateAsync: {stateName}. User: {userId}");
        // Simulate retrieving workflow definition (replace with actual logic)
        var workflowDefinition = await workflowRepository.GetWorkflowDefinitionAsync(workflowInstanceId);
        var state = workflowDefinition.States.FirstOrDefault(s => s.Name == stateName);

        if (state == null)
            return false;

        return state.Assignments.Users.Contains(userId) || state.Assignments.Groups.Contains(userId);
    }

    public async Task<IEnumerable<string>> GetAssignmentsAsync(string stateName, WorkflowInstanceId workflowInstanceId)
    {
        Console.WriteLine($"GetAssignmentsAsync: {stateName}");
        var workflowDefinition = await workflowRepository.GetWorkflowDefinitionAsync(workflowInstanceId);
        var state = workflowDefinition.States.FirstOrDefault(s => s.Name == stateName);
        
        if (state == null)
            return [];
        
        var assignedUsers = new HashSet<string>(state.Assignments.Users);

        // Add users from groups
        foreach (var group in state.Assignments.Groups)
        {
            // replace with logic to get all users in a given group
            assignedUsers.Add(group);
        }
        
        return assignedUsers;
    }
}