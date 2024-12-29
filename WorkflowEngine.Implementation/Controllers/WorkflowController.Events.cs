using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Core;

namespace WorkflowEngine.Implementation.Controllers;

public partial class WorkflowController
{
    public class WorkflowEventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public Dictionary<string, object> EventData { get; set; } = new();
    }

    [HttpPost("{instanceId:guid}/trigger")]
    public async Task<IActionResult> TriggerEvent([FromRoute] WorkflowInstanceId instanceId, [FromQuery] string actorId, [FromBody] WorkflowEventRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.EventName))
        {
            return BadRequest("Invalid event data.");
        }
        await workflowEngine.TriggerEventAsync(instanceId, request.EventName, request.EventData.ConvertJsonElements(), actorId);
        return Ok(new { Message = "Event processed successfully." });
    }

    [HttpGet("events/{instanceId}")]
    public async Task<IActionResult> GetWorkflowEvents(WorkflowInstanceId instanceId, string? eventName = null)
    {
        var retrievedEvents = await workflowEngine.GetWorkflowEventsAsync(instanceId, eventName);
        return Ok(retrievedEvents);
    }
}