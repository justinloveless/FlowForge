using FluentAssertions;

namespace WorkflowEngine.Core.UnitTests;

public class DefinitionBuilderTests
{
    [Fact]
    public async Task When_building_workflow_definition()
    {
        var builder = new WorkflowDefinitionBuilder("Sample Workflow", "Start");

        var definition = builder.AddState("Start", state =>
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
}