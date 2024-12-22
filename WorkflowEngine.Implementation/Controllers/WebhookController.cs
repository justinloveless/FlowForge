using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core;

namespace WorkflowEngine.Implementation.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController(WorkflowEngineFacade workflowEngine) : ControllerBase
{
    [HttpGet("provider/userstate/{userId}")]
    public IActionResult GetUserState(string userId)
    {
        Console.WriteLine($"GetUserState: {userId}");
        return Ok("Texas");
    }
    [HttpPost("test")]
    public IActionResult Post([FromBody] WebhookBody body)
    {
        Console.WriteLine($"Received Webhook post for workflow {body.WorkflowName} ({body.InstanceId}). Current state is {body.CurrentState}. State data is {JsonSerializer.Serialize(body.StateData)}");
        return Ok(body.StateData);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterWorkflow([FromBody] WorkflowDefinition definition)
    {
        await workflowEngine.RegisterWorkflowAsync(definition);
        return Ok(new { id = definition.Id });
    }

    [HttpPost("start/{workflowDefinitionId}")]
    public async Task<IActionResult> StartWorkflow(Guid workflowDefinitionId, [FromBody] Dictionary<string, object> initialData)
    {
        var workflowInstance = await workflowEngine.StartWorkflowAsync(workflowDefinitionId, initialData);
        return Ok(workflowInstance);
    }

    [HttpGet("instance")]
    public async Task<IActionResult> GetWorkflowInstance(Guid workflowInstanceId)
    {
        var retrievedInstance = await workflowEngine.GetWorkflowInstanceAsync(workflowInstanceId);
        return Ok(retrievedInstance);
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetWorkflowDefinitions(string workflowName)
    {
        var retrievedDefinition = await workflowEngine.GetWorkflowDefinitionsByNameAsync(workflowName);
        return Ok(retrievedDefinition);
    }

    [HttpPost("{instanceId}/trigger")]
    public async Task<IActionResult> TriggerEvent(Guid instanceId, [FromBody] WorkflowEventRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.EventName))
        {
            return BadRequest("Invalid event data.");
        }
        await workflowEngine.TriggerEventAsync(instanceId, request.EventName, request.EventData.ConvertJsonElements());
        return Ok(new { Message = "Event processed successfully." });
    }

    [HttpGet("events/{instanceId}")]
    public async Task<IActionResult> GetWorkflowEvents(Guid instanceId, string? eventName = null)
    {
        var retrievedEvents = await workflowEngine.GetWorkflowEventsAsync(instanceId, eventName);
        return Ok(retrievedEvents);
    }
    
    public class WorkflowEventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public Dictionary<string, object> EventData { get; set; } = new();
    }
}