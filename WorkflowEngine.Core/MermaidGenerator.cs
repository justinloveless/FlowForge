using System.Text;
using System.Text.Json;

namespace WorkflowEngine.Core;

public static class MermaidGenerator
{
    public static string ConvertToMermaidDiagram(WorkflowDefinition workflowDefinition, bool showDetails = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine("stateDiagram-v2");
        sb.AppendLine($"[*] --> {workflowDefinition.InitialState}");

        foreach (var state in workflowDefinition.States)
        {
            var name = IsDelayAction(workflowDefinition, state.Name) ? "Delay" 
                : IsScheduleAction(workflowDefinition, state.Name) ? "Schedule" 
                : state.Name;
            var nameLabel = $"n{Guid.NewGuid():N}";
            sb.AppendLine(nameLabel + $": {name}");
            foreach (var transition in state.Transitions) 
            {
                var condition = IsDelayAction(workflowDefinition, state.Name) ? GetDelayCondition(workflowDefinition, state.Name) 
                    : IsScheduleAction(workflowDefinition, state.Name) ? GetScheduleCondition(workflowDefinition, state.Name) 
                    : transition.Condition;
                var nextState = transition.NextState;
                if (IsDelayAction(workflowDefinition, nextState))
                    sb.AppendLine($"{name} --> Delay : {condition}");
                else if (IsScheduleAction(workflowDefinition, nextState))
                    sb.AppendLine($"{name} --> Schedule : {condition}");
                else
                    sb.AppendLine($"{name} --> {nextState} : {condition}");
            }

            if (showDetails)
            {
                GenerateStateDetails(sb, state, name);
            }
            
        }
        
        sb.AppendLine("End --> [*]");
        return sb.ToString();
    }

    private static Dictionary<string, string> GenerateStateNameLabels(WorkflowDefinition workflowDefinition)
    {
        var stateNameLabels = new Dictionary<string, string>();
        for (var index = 0; index < workflowDefinition.States.Count; index++)
        {
            var state = workflowDefinition.States[index];
            var name = IsDelayAction(workflowDefinition, state.Name) ? "Delay"
                : IsScheduleAction(workflowDefinition, state.Name) ? "Schedule"
                : state.Name;
            var nameLabel = $"state.{index}";
            stateNameLabels.Add(nameLabel, name);
        }
        return stateNameLabels;
    }

    private static Dictionary<string, string> GenerateActionLabels(StateDefinition stateDefinition, int stateIndex)
    {
        var actionLabels = new Dictionary<string, string>();
        actionLabels.Add($"OnEnter.{stateIndex}", "OnEnter");
        actionLabels.Add($"OnExit.{stateIndex}", "OnExit");
        for (int actionIndex = 0; actionIndex < stateDefinition.OnEnterActions.Count; actionIndex++)
        {
            foreach (var parameter in stateDefinition.OnEnterActions[actionIndex].Parameters)
            {
                actionLabels.Add($"OnEnterAction.{stateIndex}.{actionIndex}.{parameter.Key.Sanitize()}", parameter.Key.Sanitize() + "#58;#32;" + parameter.Value.ToString().Sanitize());
            }
        }
        
        return actionLabels;
    }

    private static string Sanitize(this string input)
    {
        return input
            .Replace(":", "#58;")
            .Replace(" ", "#32;")
            .Replace("-", "#45;");
    }
    
    private static void GenerateStateDetails(StringBuilder sb, StateDefinition state, string name)
    {
        sb.AppendLine($"state {name} {{");
        var onEnterLabel = $"onEnter{Guid.NewGuid():N}";
        sb.AppendLine(onEnterLabel + ": OnEnter");
        var onExitLabel = $"onExit{Guid.NewGuid():N}";
        sb.AppendLine(onExitLabel + ": OnExit");
        // Add OnEnter actions
        if (state.OnEnterActions?.Count > 0)
        {
            sb.AppendLine($"    [*] --> {onEnterLabel}");
            sb.AppendLine($"    state {onEnterLabel} {{");
            sb.AppendLine("        direction LR");
            
            foreach (var action in state.OnEnterActions)
            {
                sb.AppendLine($"            {FormatAction(action)}");
            }
            sb.AppendLine("        }");
        }
        
        // Add OnExit actions
        if (state.OnExitActions?.Count > 0)
        {
            if (state.OnEnterActions?.Count > 0)
            {
                sb.AppendLine($"        {onEnterLabel} --> {onExitLabel}");
            }
            sb.AppendLine($"        state {onExitLabel} {{");
            sb.AppendLine("            direction LR");
            for (var index = 0; index < state.OnExitActions.Count; index++)
            {
                if (index > 0)
                {
                    sb.AppendLine($"            --");    
                }
                var action = state.OnExitActions[index];
                sb.AppendLine($"            {FormatAction(action)}");
            }

            sb.AppendLine("        }");
        }
        
        // Add transitions between OnEnter and OnExit
        if (state.OnEnterActions?.Count > 0 && state.OnExitActions?.Count > 0)
        {
            sb.AppendLine($"        {onExitLabel} --> [*]");
        }

        sb.AppendLine("    }");
    }
    
    private static string FormatAction(WorkflowAction action)
    {
        
        return string.Join("\n", action.Parameters.Select(kvp =>
        {
            var label = "p" + Guid.NewGuid().ToString("N");
            return $"{label}: " +kvp.Key + "#58;#32;" + kvp.Value?.ToString()?
                .Replace(":", "#58;")
                .Replace(" ", "#32;")
                .Replace("-", "#45;")
                + $"\n{label} ";
        }));
    }
    
    private static string GetScheduleCondition(WorkflowDefinition workflowDefinition, string name) =>
        GetTimerConditionString(workflowDefinition, name, "absoluteSchedule");
    private static string GetDelayCondition(WorkflowDefinition workflowDefinition, string name) => 
        GetTimerConditionString(workflowDefinition, name, "relativeDelay");

    private static string GetTimerConditionString(WorkflowDefinition workflowDefinition, string stateName, string parameterName)
    {
        var state = workflowDefinition.States.FirstOrDefault(s => s.Name == stateName);
        if (state == null) return "";

        var action = state.OnEnterActions.FirstOrDefault(a => a.Type == "Timer");
        action.Parameters.TryGetValue(parameterName, out var relativeDelay);
        return relativeDelay?.ToString().Replace(":", "#58;") ?? "";
    }

    private static bool IsDelayAction(WorkflowDefinition definition, string stateName)
    {
        return IsTimerAction(definition, stateName, "relativeDelay");
    }

    private static bool IsScheduleAction(WorkflowDefinition definition, string stateName)
    {
        return IsTimerAction(definition, stateName, "absoluteSchedule");
    }

    private static bool IsTimerAction(WorkflowDefinition definition, string stateName, string parameterName)
    {
        var state = definition.States.FirstOrDefault(s => s.Name == stateName);
        return state?.OnEnterActions != null 
               && state.OnEnterActions.Where(action => action.Type == "Timer")
                   .Any(action => action.Parameters.TryGetValue(parameterName, out _));
    }
}