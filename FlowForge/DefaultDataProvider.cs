﻿namespace FlowForge;

internal class DefaultDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;

    public DefaultDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<object> GetDataAsync(string urlTemplate, WorkflowInstanceId instanceId, 
        Dictionary<string, object> instanceData, Dictionary<string, object> stateData)
    {
        var url = urlTemplate.Replace("{instanceId}", instanceId.ToString());
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }

}