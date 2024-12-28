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
        var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl);
        request.Content = JsonContent.Create(new WebhookBody(
            instance.WorkflowName, 
            instance.Id, 
            instance.CurrentState, 
            instance.StateData));
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("X-API-KEY", "abcd1234");
        var res = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        res.EnsureSuccessStatusCode();
        
        var responseData = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return responseData.ConvertJsonElements();
    }

    
}

public record WebhookBody (string WorkflowName, WorkflowInstanceId InstanceId, string CurrentState, Dictionary<string, object> StateData);