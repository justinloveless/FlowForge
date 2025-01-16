using Microsoft.EntityFrameworkCore;
using FlowForge;

namespace FlowForge.Postgresql;

public class PostgresqlWorkflowRepository(WorkflowDbContext dbContext) : IWorkflowRepository
{
    public async Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        if (workflow.Id.Equals(Guid.Empty))
            throw new ArgumentNullException(nameof(workflow), "Workflow Id cannot be empty.");

        var exists = await dbContext.WorkflowDefinitions.AnyAsync(w => w.Id == workflow.Id);
        if (exists)
            throw new InvalidOperationException($"A workflow with the Id '{workflow.Name}' is already registered.");

        dbContext.WorkflowDefinitions.Add(workflow);
        await dbContext.SaveChangesAsync();
    }

    public async Task<WorkflowInstance> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData)
    {
        var workflowDefinition = await dbContext.WorkflowDefinitions.FindAsync(workflowId);
        if (workflowDefinition == null)
            throw new InvalidOperationException($"No workflow found with the Id '{workflowId}'.");

        var workflowInstance = new WorkflowInstance
        {
            WorkflowName = workflowDefinition.Name,
            DefinitionId = workflowDefinition.Id,
            ActiveStates = [workflowDefinition.InitialState],
            WorkflowData = initialData
        };

        dbContext.WorkflowInstances.Add(workflowInstance);
        await dbContext.SaveChangesAsync();

        return workflowInstance;
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceAsync(WorkflowInstanceId instanceId)
    {
        return await dbContext.WorkflowInstances.FindAsync(instanceId)
               ?? throw new InvalidOperationException($"No workflow instance found with the ID '{instanceId}'.");
    }

    public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByDefinitionIdAsync(WorkflowDefinitionId definitionId)
    {
        return await dbContext.WorkflowInstances.Where(i => i.DefinitionId == definitionId).ToListAsync();
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetEventDrivenWorkflowDefinitionsAsync(string eventName)
    {
        return await dbContext.WorkflowDefinitions.Include(d => d.States)
            .ThenInclude(s => s.Transitions)
            .Where(d => d.IsEventDriven && d.States.Any(s => s.Name == d.InitialState && 
                                                             s.Transitions.Any(t => t.Condition.Contains(eventName))))
            .ToListAsync();
    }


    public async Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowInstanceId instanceId)
    {
        var instance = await dbContext.WorkflowInstances.FindAsync(instanceId);
        if (instance == null)
            throw new InvalidOperationException($"No workflow instance found with the ID '{instanceId}'.");

        var definition = await dbContext.WorkflowDefinitions.FindAsync(instance.DefinitionId);
        if (definition == null)
            throw new InvalidOperationException($"No workflow definition found with the ID '{instance.DefinitionId}'.");

        return definition;
    }
    public async Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowDefinitionId definitionId)
    {
        var definition = await dbContext.WorkflowDefinitions.FindAsync(definitionId);
        if (definition == null)
            throw new InvalidOperationException($"No workflow definition found with the ID '{definitionId}'.");

        return definition;
    }

    public async Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(string name)
    {
        return await dbContext.WorkflowDefinitions
            .Where(w => w.Name == name)
            .ToListAsync();
    }

    public async Task UpdateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        var existingInstance = await dbContext.WorkflowInstances.FindAsync(instance.Id);
        if (existingInstance == null)
            throw new InvalidOperationException($"No workflow instance found with the ID '{instance.Id}'.");

        dbContext.Entry(existingInstance).CurrentValues.SetValues(instance);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateWorkflowDefinitionAsync(WorkflowDefinition workflow)
    {
        var existingDefinition = await dbContext.WorkflowDefinitions.FindAsync(workflow.Id);
        if (existingDefinition == null)
            throw new InvalidOperationException($"No workflow definition found with the ID '{workflow.Id}'.");

        dbContext.Entry(existingDefinition).CurrentValues.SetValues(workflow);
        await dbContext.SaveChangesAsync();
    }
}