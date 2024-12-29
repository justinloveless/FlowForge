using FluentAssertions;

namespace WorkflowEngine.Core.UnitTests;

public class DefinitionBuilderTests
{
    [Fact]
    public async Task When_building_workflow_definition()
    {
        var definition = new WorkflowDefinitionBuilder("Sample Workflow", "Start")
            .AddState("Start", state =>
        {
            state
                .AddOnEnterAction(new WebhookAction("http://example.com"))
                .AddTransition("true", "Middle");
        })
        .AddState("TimerState", state =>
        {
            state
                .SetIdle(true)
                .AddOnEnterAction(new TimerAction(TimeSpan.FromHours(12)))
                .AddTransition("true", "End");
        })
        .AddState("End", state =>
        {
            state.AddOnEnterAction(new WebhookAction("http://example.com"));
        }).Build();

        definition.Name.Should().Be("Sample Workflow");
        definition.InitialState.Should().Be("Start");
        definition.States.Count.Should().Be(3);
        definition.States[0].Name.Should().Be("Start");
        definition.States[0].OnEnterActions.Count.Should().Be(1);
        definition.States[0].OnEnterActions[0].Type.Should().Be("Webhook");
        definition.States[0].OnEnterActions[0].Parameters["url"].Should().Be("http://example.com");
        definition.States[0].Transitions.Count.Should().Be(1);
        definition.States[0].Transitions[0].NextState.Should().Be("Middle");
    }

    [Fact]
    public async Task When_building_workflow_definition_v2()
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
        
        var json = definition.ToJson();
        definition.Name.Should().Be("Sample Workflow");
        definition.InitialState.Should().Be("Start");
        definition.States.Count.Should().Be(8);
        definition.States[0].Name.Should().Be("Start");
        definition.States[0].OnEnterActions.Count.Should().Be(1);
        definition.States[0].OnEnterActions[0].Type.Should().Be("Webhook");
        definition.States[0].OnEnterActions[0].Parameters["url"].Should().Be("http://example.com");
        definition.States[0].Transitions.Count.Should().Be(1);
        definition.States[1].Transitions[0].NextState.Should().Be("UserStep");
        definition.States[2].Transitions[0].NextState.Should().Be("Approval");
        definition.States[3].Transitions[0].NextState.Should().Be("State4");
        definition.States[3].Transitions[1].NextState.Should().Be("UserStep");
        
    }
}