namespace WorkflowEngine.Core.UnitTests;

public class MermaidGeneratorTests
{
    [Fact]
    public async Task When_generating_mermaid_diagram()
    {
        var definition = new WorkflowDefinitionBuilder("Sample Workflow")
            .Start(s => s
                .OnEnter(new WebhookAction("http://example.com"))) // automatically create the Start state
            .Delay(TimeSpan.FromMinutes(12))
            .ActionableStep("UserStep", s => s // actionable steps should set IsIdle to true
                .AssignUser("justin")
                .OnEnter(new WebhookAction("http://notify.me"))
                .Transition("event == \"Submit\"") // just goes to next state
                .OnExit(new WebhookAction("http://something.com"))
            )
            .ActionableStep("Approval", s => s
                .AssignGroup("managers")
                .OnEnter(new WebhookAction("http://notify.me"))    
                .Transition("event == \"Approved\"") // just goes to next state
                .Transition("event == \"Declined\"", "UserStep")
            )
            .Schedule(new DateTime(2025,01,01))
            .ActionableStep("RenewLicense", s => s
                .AssignUser("justin")
                .OnEnter(new WebhookAction("http://notify.me"))
                .Transition("event == \"Submit\"")
            )
            .ActionableStep("ApproveLicenseRenewal", s => s
                .Transition("event == \"Approved\"")
                .Transition("event == \"Declined\"", "RenewLicense")
            )
            .End(s => s
                .OnEnter(new WebhookAction("http://notify.me"))
            ); // automatically create the End state and then build

        var mermaidDiagram = MermaidGenerator.ConvertToMermaidDiagram(definition);
        
    }
}