using System.Text.Json;
using WorkflowEngine.Core;

namespace WorkflowEngine.Implementation;

public class StateDataProvider : BaseDataProvider
{
    public StateDataProvider(HttpClient httpClient) : base(httpClient)
    {
    }

    public StateDataProvider(HttpClient httpClient, Dictionary<string, string> variableToUrlMapping) : base(httpClient,
        variableToUrlMapping)
    {
    }

    protected override string ReplacePlaceholders(string urlTemplate, Guid instanceId, Dictionary<string, object> stateData)
    {
        // Replace additional placeholders, if needed
        var updatedUrl = base.ReplacePlaceholders(urlTemplate, instanceId, stateData);

        if (stateData.TryGetValue("userId", out var userId))
        {
            updatedUrl = updatedUrl.Replace("{userId}", userId.ToString());
        }

        return updatedUrl;
    }
    
    protected override object DeserializeResult(string result)
    {
        // Example: Parse JSON response into an object
        return JsonSerializer.Deserialize<object>(result);
    }
}