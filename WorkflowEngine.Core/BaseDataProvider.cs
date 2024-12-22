namespace WorkflowEngine.Core;

public class BaseDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;
    protected Dictionary<string, string> _variableToUrlMapping = new();
    
    public BaseDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public BaseDataProvider(HttpClient httpClient, Dictionary<string, string> variableToUrlMapping)
    {
        _httpClient = httpClient;
        _variableToUrlMapping = variableToUrlMapping;
    }
    public void Configure(Dictionary<string, string> variableToUrlMapping)
    {
        _variableToUrlMapping = variableToUrlMapping;
    }

    public async Task<object> GetDataAsync(string urlTemplate, Guid instanceId, Dictionary<string, object> stateData)
    {
        // Replace placeholders in the URL template with actual data
        var url = ReplacePlaceholders(urlTemplate, instanceId, stateData);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        return DeserializeResult(result);
    }

    public bool HasKey(string key)
    {
        return _variableToUrlMapping.ContainsKey(key);
    }
    
    protected virtual string ReplacePlaceholders(string urlTemplate, Guid instanceId, Dictionary<string, object> stateData)
    {
        // Example: Replace {instanceId} and other placeholders in the URL
        return urlTemplate.Replace("{instanceId}", instanceId.ToString());
    }

    protected virtual object DeserializeResult(string result)
    {
        // Customize deserialization logic if needed
        return result;
    }
}