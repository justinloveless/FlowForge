using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core;

namespace WorkflowEngine.Implementation.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    [HttpPost("test")]
    public IActionResult Post([FromBody] WebhookBody body)
    {
        Console.WriteLine($"Received Webhook post for workflow {body.WorkflowName} ({body.InstanceId}). Current state is {body.CurrentState}. State data is {JsonSerializer.Serialize(body.StateData)}");
        return Ok(body.StateData);
    }

}