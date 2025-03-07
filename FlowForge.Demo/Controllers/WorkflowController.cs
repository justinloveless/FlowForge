﻿using Microsoft.AspNetCore.Mvc;
using FlowForge.Mermaid;

namespace FlowForge.Demo.Controllers;


[ApiController]
[Route("[controller]")]
public partial class WorkflowController(FlowForge flowForge) : ControllerBase
{
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterWorkflow([FromBody] WorkflowDefinition definition)
    {
        await flowForge.RegisterWorkflowAsync(definition);
        return Ok(definition.Id);
    }
    
    [HttpPost("register/predefined")]
    public async Task<IActionResult> RegisterPredefinedWorkflow()
    {
        var def = new WorkflowDefinitionBuilder("Sample Event Driven Workflow")
            .AsEventDriven()
            .Start(s => s
                .OnEnter(new WebhookAction("http://localhost:8080/webhook/test"))
                .Transition("event == \"UsernameChanged\"", "UserStep")
            )
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
        await flowForge.RegisterWorkflowAsync(def);
        return Ok(def.Id);
    }
    
    
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateWorkflow([FromBody] WorkflowDefinition definition)
    {
        await flowForge.UpdateWorkflowDefinitionAsync(definition);
        return Ok();
    }
    

    [HttpPost("start/{workflowDefinitionId:guid}")]
    public async Task<IActionResult> StartWorkflow([FromRoute] WorkflowDefinitionId workflowDefinitionId, [FromBody] Dictionary<string, object> initialData)
    {
        var workflowInstance = await flowForge.StartWorkflowAsync(workflowDefinitionId, initialData);
        return Ok(workflowInstance);
    }

    [HttpGet("instance")]
    public async Task<IActionResult> GetWorkflowInstance(WorkflowInstanceId workflowInstanceId)
    {
        var retrievedInstance = await flowForge.GetWorkflowInstanceAsync(workflowInstanceId);
        return Ok(retrievedInstance);
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetWorkflowDefinitions(string workflowName)
    {
        var retrievedDefinition = await flowForge.GetWorkflowDefinitionsByNameAsync(workflowName);
        return Ok(retrievedDefinition);
    }

    [HttpGet("{instanceId:guid}/assignments/{stateName}")]
    public async Task<IActionResult> GetAssignedUsers(WorkflowInstanceId instanceId, string stateName)
    {
        var users = await flowForge.GetAssignedActorsAsync(stateName, instanceId);
        return Ok(users);
    }

    [HttpGet("mermaid/generate/{workflowDefinitionId:guid}")]
    public async Task<IActionResult> GetMermaidDiagram(WorkflowDefinitionId workflowDefinitionId, [FromQuery] bool showDetails = false)
    {
        var definition = await flowForge.GetWorkflowDefinitionByIdAsync(workflowDefinitionId);
        var mermaidDiagram = await flowForge.ConvertToMermaidAsync(definition, showDetails);
        return Ok(mermaidDiagram);
    }
    
}