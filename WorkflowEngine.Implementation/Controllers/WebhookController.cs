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
        return Ok(
            """
            {
            "state": "Texas"
            }
            """
        );
    }
    [HttpGet("provider/userage/{userId}")]
    public IActionResult GetUserAge(string userId)
    {
        Console.WriteLine($"GetUserAge: {userId}");
        return Ok(
            """
            {
            "age": 19
            }
            """
            );
    }
    
    [HttpGet("provider/trainings/{instanceId:guid}")]
    public IActionResult GetTrainings(Guid instanceId)
    {
        Console.WriteLine($"GetTrainings: {instanceId}");
        var result = new
        {
            Classes = new List<Training>()
            {
                new() {
                    Name = "How to get gud",
                    Cost = 420,
                    CompletedOn = DateTime.Now,
                },
                
                new() {
                    Name = "How to get gud",
                    Cost = 420,
                    CompletedOn = DateTime.Now,
                }
            },
            UserId = instanceId.ToString(),
            UserName = "John Doe",
        };
        
        return Ok(result);
    }

    public class Training
    {
        public string Name { get; set; }
        public double Cost { get; set; }
        public DateTime CompletedOn { get; set; }
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
        return Ok(definition.Id);
    }
    
    [HttpPost("register/predefined")]
    public async Task<IActionResult> RegisterPredefinedWorkflow()
    {
        var def = new WorkflowDefinitionBuilder("Sample Predefined Workflow")
            .Start(s => s
                .OnEnter(new WebhookAction("http://localhost:8080/webhook/test")))
            .Delay(TimeSpan.FromSeconds(10))
            .ActionableStep("UserStep", s => s 
                .AssignUser("justin")
                .OnEnter(new WebhookAction("http://localhost:8080/webhook/test"))
                .Transition("event == \"Submit\"")
                .OnExit(new WebhookAction("http://localhost:8080/webhook/test"))
            )
            .ActionableStep("Approval", s => s
                .AssignGroup("managers")
                .OnEnter(new WebhookAction("http://localhost:8080/webhook/test"))    
                .Transition("event == \"Approved\"") 
                .Transition("event == \"Declined\"", "UserStep")
            )
            .Schedule(new DateTime(2025,01,01))
            .ActionableStep("RenewLicense", s => s
                .AssignUser("justin")
                .OnEnter(new WebhookAction("http://localhost:8080/webhook/test"))
                .Transition("event == \"Submit\"")
            )
            .ActionableStep("ApproveLicenseRenewal", s => s
                .OnEnter(CustomActions.MyCustomAction)
                .Transition("event == \"Approved\"")
                .Transition("event == \"Declined\"", "RenewLicense")
            )
            .End(); 
        await workflowEngine.RegisterWorkflowAsync(def);
        return Ok(def.Id);
    }
    
    
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateWorkflow([FromBody] WorkflowDefinition definition)
    {
        await workflowEngine.UpdateWorkflowDefinitionAsync(definition);
        return Ok();
    }
    

    [HttpPost("start/{workflowDefinitionId:guid}")]
    public async Task<IActionResult> StartWorkflow([FromRoute] WorkflowDefinitionId workflowDefinitionId, [FromBody] Dictionary<string, object> initialData)
    {
        var workflowInstance = await workflowEngine.StartWorkflowAsync(workflowDefinitionId, initialData);
        return Ok(workflowInstance);
    }

    [HttpGet("instance")]
    public async Task<IActionResult> GetWorkflowInstance(WorkflowInstanceId workflowInstanceId)
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

    [HttpGet("{instanceId:guid}/assignments/{stateName}")]
    public async Task<IActionResult> GetAssignedUsers(WorkflowInstanceId instanceId, string stateName)
    {
        var users = await workflowEngine.GetAssignedActorsAsync(stateName, instanceId);
        return Ok(users);
    }
    
    public class WorkflowEventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public Dictionary<string, object> EventData { get; set; } = new();
    }
}