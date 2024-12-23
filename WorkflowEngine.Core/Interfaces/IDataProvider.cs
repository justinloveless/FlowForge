namespace WorkflowEngine.Core;

public interface IDataProvider
{
    Task<object> GetDataAsync(string urlTemplate, WorkflowInstanceId instanceId, Dictionary<string, object> stateData);
}