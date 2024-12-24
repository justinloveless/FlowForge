using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core;

namespace WorkflowEngine.Postgresql;

public class PostgresqlWorkflowRepository : IWorkflowRepository
{
    private readonly WorkflowDbContext _dbContext;

    public PostgresqlWorkflowRepository(WorkflowDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task RegisterWorkflowAsync(WorkflowDefinition workflow)
    {
        if (workflow.Id.Equals(Guid.Empty))
            throw new ArgumentNullException(nameof(workflow), "Workflow Id cannot be empty.");

        var exists = await _dbContext.WorkflowDefinitions.AnyAsync(w => w.Id == workflow.Id);
        if (exists)
            throw new InvalidOperationException($"A workflow with the Id '{workflow.Name}' is already registered.");

        _dbContext.WorkflowDefinitions.Add(workflow);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<WorkflowInstance> StartWorkflowAsync(WorkflowDefinitionId workflowId, Dictionary<string, object> initialData)
    {
        var workflowDefinition = await _dbContext.WorkflowDefinitions.FindAsync(workflowId);
        if (workflowDefinition == null)
            throw new InvalidOperationException($"No workflow found with the Id '{workflowId}'.");

        var workflowInstance = new WorkflowInstance
        {
            WorkflowName = workflowDefinition.Name,
            DefinitionId = workflowDefinition.Id,
            CurrentState = workflowDefinition.InitialState,
            WorkflowData = initialData
        };

        _dbContext.WorkflowInstances.Add(workflowInstance);
        await _dbContext.SaveChangesAsync();

        return workflowInstance;
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceAsync(WorkflowInstanceId instanceId)
    {
        return await _dbContext.WorkflowInstances.FindAsync(instanceId)
               ?? throw new InvalidOperationException($"No workflow instance found with the ID '{instanceId}'.");
    }

    public async Task<WorkflowDefinition> GetWorkflowDefinitionAsync(WorkflowInstanceId instanceId)
    {
        var instance = await _dbContext.WorkflowInstances.FindAsync(instanceId);
        if (instance == null)
            throw new InvalidOperationException($"No workflow instance found with the ID '{instanceId}'.");

        var definition = await _dbContext.WorkflowDefinitions.FindAsync(instance.DefinitionId);
        if (definition == null)
            throw new InvalidOperationException($"No workflow definition found with the ID '{instance.DefinitionId}'.");

        return definition;
    }

    public async Task<List<WorkflowDefinition>> GetWorkflowDefinitionsAsync(string name)
    {
        return await _dbContext.WorkflowDefinitions
            .Where(w => w.Name == name)
            .ToListAsync();
    }

    public async Task UpdateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        var existingInstance = await _dbContext.WorkflowInstances.FindAsync(instance.Id);
        if (existingInstance == null)
            throw new InvalidOperationException($"No workflow instance found with the ID '{instance.Id}'.");

        _dbContext.Entry(existingInstance).CurrentValues.SetValues(instance);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateWorkflowDefinitionAsync(WorkflowDefinition workflow)
    {
        var existingDefinition = await _dbContext.WorkflowDefinitions.FindAsync(workflow.Id);
        if (existingDefinition == null)
            throw new InvalidOperationException($"No workflow definition found with the ID '{workflow.Id}'.");

        _dbContext.Entry(existingDefinition).CurrentValues.SetValues(workflow);
        await _dbContext.SaveChangesAsync();
    }
}