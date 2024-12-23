using System.Net.Http.Json;

namespace WorkflowEngine.Core;

internal class WebhookHandler: IWebhookHandler
{
    private readonly HttpClient _httpClient;

    public WebhookHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, object>> CallWebhookAsync(string webhookUrl, WorkflowInstance instance)
    {
        var response = await _httpClient.PostAsJsonAsync(webhookUrl, new WebhookBody(
            instance.WorkflowName, 
            instance.Id, 
            instance.CurrentState, 
            instance.StateData));
        
        response.EnsureSuccessStatusCode();
        
        var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return responseData.ConvertJsonElements();
    }

    
}

public record WebhookBody (string WorkflowName, WorkflowInstanceId InstanceId, string CurrentState, Dictionary<string, object> StateData);