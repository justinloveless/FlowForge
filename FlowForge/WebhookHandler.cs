using System.Net.Http.Json;

namespace FlowForge;

internal class WebhookHandler(HttpClient httpClient, IWebhookRegistry webhookRegistry, IWorkflowEngine engine) : IWebhookHandler
{
    public async Task<Dictionary<string, object>> CallWebhookAsync(string webhookUrl, Dictionary<string, object> headers,
        WorkflowInstance instance)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl);
        request.Content = JsonContent.Create(new WebhookBody(
            instance.WorkflowName, 
            instance.Id, 
            instance.ActiveStates, 
            instance.StateData));
        request.Headers.Add("Accept", "application/json");
        foreach (var hkey in headers.Keys)
        {
            request.Headers.Add(hkey, headers[hkey].ToString());
        }
        var res = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        res.EnsureSuccessStatusCode();
        
        var responseData = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return responseData.ConvertJsonElements();
    }

    public async Task HandleWebhookAsync(WebhookRegistrationId webhookId, Dictionary<string, object> webhookData)
    {
        var webhook = await webhookRegistry.GetWebhookRegistrationAsync(webhookId);
        if (webhook is null) return;

        await engine.TriggerEventForWorkflowDefinitionAsync(webhook.WorkflowDefinitionId, webhook.EventName,
            webhookData);


    }

    
}

public record WebhookBody (string WorkflowName, WorkflowInstanceId InstanceId, List<string> ActiveStates, Dictionary<string, object> StateData);