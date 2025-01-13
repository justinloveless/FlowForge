namespace FlowForge;

public class ScheduleEvent
{
    public ScheduleEventId Id { get; init; } = new (Guid.NewGuid());
    public WorkflowInstanceId InstanceId { get; set; }
    public DateTime ResumeTime { get; set; }
    public string EventName { get; set; }
}

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct ScheduleEventId;