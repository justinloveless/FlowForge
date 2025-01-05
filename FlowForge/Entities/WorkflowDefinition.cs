namespace FlowForge;

public class WorkflowDefinition
{
    public WorkflowDefinitionId Id { get; } = new (Guid.NewGuid());
    public string Name { get; set; } = string.Empty;
    public string InitialState { get; set; } = string.Empty;
    public List<StateDefinition> States { get; set; } = [];
    public bool IsEventDriven { get; set; } = false;
}

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct WorkflowDefinitionId;
