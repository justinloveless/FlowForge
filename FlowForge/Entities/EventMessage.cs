namespace FlowForge;

public class EventMessage
{
    public WorkflowInstanceId? WorkflowInstanceId { get; set; }
    public string EventName { get; set; }
    public Dictionary<string, object> EventData { get; set; }
    
}