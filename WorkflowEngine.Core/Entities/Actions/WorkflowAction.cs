namespace WorkflowEngine.Core;

public  class WorkflowAction
{
    public string Type { get; set; }
    public IDictionary<string, object> Parameters { get; set; }
}