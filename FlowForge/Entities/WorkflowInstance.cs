namespace FlowForge;

public class WorkflowInstance
{
    public WorkflowInstanceId Id { get; init; } = new (Guid.NewGuid());
    public WorkflowDefinitionId DefinitionId { get; init; }
    public string WorkflowName { get; init; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public Dictionary<string, object> StateData { get; set; } = new();
    public Dictionary<string, object> WorkflowData { get; init; } = new();
}

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct WorkflowInstanceId;