using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FlowForge.Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    [HttpPost("test")]
    public IActionResult Post([FromBody] WebhookBody body)
    {
        Console.WriteLine($"Received Webhook post for workflow {body.WorkflowName} ({body.InstanceId}). Current active states are {string.Join(", ", body.ActiveStates)}. State data is {JsonSerializer.Serialize(body.StateData)}");
        return Ok(body.StateData);
    }

}