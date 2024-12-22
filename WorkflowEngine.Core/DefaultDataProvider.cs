namespace WorkflowEngine.Core;

public class DefaultDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;

    public DefaultDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<object> GetDataAsync(string urlTemplate, Guid instanceId, Dictionary<string, object> stateData)
    {
        var url = urlTemplate.Replace("{instanceId}", instanceId.ToString());
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }

}