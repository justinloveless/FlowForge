using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController(FlowForge flowForge) : ControllerBase
{
    [HttpPost("test")]
    public IActionResult Post([FromBody] WebhookBody body)
    {
        Console.WriteLine($"Received Webhook post for workflow {body.WorkflowName} ({body.InstanceId}). Current active states are {string.Join(", ", body.ActiveStates)}. State data is {JsonSerializer.Serialize(body.StateData)}");
        return Ok(body.StateData);
    }

    [HttpPost("{webhookId:guid}")]
    public async Task<IActionResult> HandleWebhook(WebhookRegistrationId webhookId,
        [FromBody] Dictionary<string, object> webhookData)
    {
        await flowForge.HandleWebhook(webhookId, webhookData);
        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterWebhook([FromBody] WebhookRegistrationDto registrationDto)
    {
        var webhookId = await flowForge.RegisterWebhook(registrationDto.WorkflowDefinitionId, registrationDto.EventName);
        return Ok(webhookId);
    }

    public class WebhookRegistrationDto
    {
        public WorkflowDefinitionId WorkflowDefinitionId { get; set; }
        public string EventName { get; set; }
    }

}