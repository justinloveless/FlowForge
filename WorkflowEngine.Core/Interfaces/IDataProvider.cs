namespace WorkflowEngine.Core;

public interface IDataProvider
{
    Task<object> GetDataAsync(string urlTemplate, Guid instanceId, Dictionary<string, object> stateData);
}