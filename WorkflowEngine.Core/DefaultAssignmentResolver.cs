namespace WorkflowEngine.Core;

public class DefaultAssignmentResolver : IAssignmentResolver
{
    private readonly IWorkflowRepository _workflowRepository;

    public DefaultAssignmentResolver(IWorkflowRepository workflowRepository)
    {
        _workflowRepository = workflowRepository;
    }
    
    public async Task<bool> CanActOnStateAsync(string stateName, WorkflowInstanceId workflowInstanceId, string userId)
    {
        
        // Simulate retrieving workflow definition (replace with actual logic)
        var workflowDefinition = await _workflowRepository.GetWorkflowDefinitionAsync(workflowInstanceId);
        var state = workflowDefinition.States.FirstOrDefault(s => s.Name == stateName);

        if (state == null)
            return false;

        return state.Assignments.Users.Contains(userId) || state.Assignments.Groups.Contains(userId);
    }

    public async Task<IEnumerable<string>> GetAssignmentsAsync(string stateName, WorkflowInstanceId workflowInstanceId)
    {
        var workflowDefinition = await _workflowRepository.GetWorkflowDefinitionAsync(workflowInstanceId);
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