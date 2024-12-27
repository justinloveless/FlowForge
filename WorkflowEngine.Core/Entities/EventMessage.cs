namespace WorkflowEngine.Core;

public class EventMessage
{
    public string WorkflowInstanceId { get; set; }
    public string EventName { get; set; }
    public Dictionary<string, object> EventData { get; set; }
    
}