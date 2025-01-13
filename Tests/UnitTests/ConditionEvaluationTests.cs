using FluentAssertions;
using Moq;
using FlowForge;

namespace Tests.UnitTests;

public class ConditionEvaluationTests
{
    private readonly Mock<IEventLogger> _eventLogger;
    private readonly Mock<IDataProvider> _dataProvider;
    private readonly VariableUrlMappings _variableUrlMappings;
    private readonly ConditionEngine _conditionEngine;
    
    public ConditionEvaluationTests()
    {
        _eventLogger = new Mock<IEventLogger>();
        _dataProvider = new Mock<IDataProvider>();
        _variableUrlMappings = new VariableUrlMappings();
        
        _conditionEngine =
            new ConditionEngine(_eventLogger.Object, _dataProvider.Object, _variableUrlMappings);
    }

    public static IEnumerable<object[]> GoodData()
    {
        yield return ["true", new Dictionary<string, object>{}, true];
        yield return ["false", new Dictionary<string, object>{}, false];
        yield return ["userInput == \"approved\"", new Dictionary<string, object>{{"userInput", "approved"}}, true];
        yield return ["userInput == \"approved\"", new Dictionary<string, object>{{"userInput", "pending"}}, false];
        yield return ["ready == true", new Dictionary<string, object>{{"userInput", "pending"}, {"ready", true}}, true];
        yield return ["ready == true && userInput == \"approved\"", new Dictionary<string, object>{{"userInput", "pending"}, {"ready", true}}, false];
        yield return ["status == initialStatus", new Dictionary<string, object>{{"initialStatus", "pending"}, {"status", "pending"}}, true];
        yield return ["status == initialStatus", new Dictionary<string, object>{{"initialStatus", "pending"}, {"status", null}}, false];

    }
    [Theory]
    [MemberData(nameof(GoodData))]
    public async Task When_evaluating_valid_conditions(string condition, Dictionary<string, object> value, bool expectedResult)
    {
        var instance = new WorkflowInstance()
        {
            Id = new WorkflowInstanceId(Guid.Empty),
            StateData = value,
        };
        var result = await _conditionEngine.EvaluateCondition(condition, instance, "", "");
        result.Should().Be(expectedResult);
        
    }
    
    
    public static IEnumerable<object[]> BadData()
    {
        yield return ["\"something\"; return true;", new Dictionary<string, object>()];
        yield return ["x = 1;", new Dictionary<string, object>()];
        yield return ["ready == true", new Dictionary<string, object>{{"userInput", "pending"}, {"ready", null}}];
        yield return ["ready == true", new Dictionary<string, object>{{"userInput", "pending"}}];
        yield return ["status == invalidKey", new Dictionary<string, object>{{"initialStatus", "pending"}, {"status", "pending"}}];
    }
    [Theory]
    [MemberData(nameof(BadData))]
    public void When_evaluating_bad_conditions(string condition, Dictionary<string, object> value)
    {
        var instance = new WorkflowInstance()
        {
            Id = new WorkflowInstanceId(Guid.Empty),
            StateData = value,
        };
        Assert.ThrowsAnyAsync<Exception>(async () => await _conditionEngine.EvaluateCondition(condition, instance, "", ""));
        
    }

    public static IEnumerable<object[]> MissingVariables()
    {
        yield return ["userState.state == \"Texas\"", "userState", "{\"state\": \"Texas\"}", new Dictionary<string, object>{}, true];
        yield return ["userAge.age >= 18", "userAge", "{\"age\": 18}", new Dictionary<string, object>{}, true];
    }

    [Theory]
    [MemberData(nameof(MissingVariables))]
    public async Task When_evaluating_conditions_with_runtime_data_providers(string condition, string variableName, object valueToReturn,
        Dictionary<string, object> stateData, bool expectedResult)
    {
        _variableUrlMappings.AddMapping(variableName, "https://some/url/template/with/{instanceId}");
        _dataProvider.Setup(d =>
                d.GetDataAsync(It.IsAny<string>(), It.IsAny<WorkflowInstanceId>(), 
                    It.IsAny<Dictionary<string, object>>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.FromResult(valueToReturn));

        var instance = new WorkflowInstance()
        {
            Id = new WorkflowInstanceId(Guid.Empty),
            StateData = stateData,
        };
        var result = await _conditionEngine.EvaluateCondition(condition, instance, "", "");
        result.Should().Be(expectedResult);
    }

}