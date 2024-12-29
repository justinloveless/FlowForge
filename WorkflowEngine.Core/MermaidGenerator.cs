using System.Text;
using System.Text.Json;

namespace WorkflowEngine.Core;

public static class MermaidGenerator
{
    public static string ConvertToMermaidDiagram(WorkflowDefinition workflowDefinition, bool showDetails = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine("stateDiagram-v2");
        var labels = GenerateStateNameLabels(workflowDefinition);
        if (showDetails)
        {
            for (var index = 0; index < workflowDefinition.States.Count; index++)
            {
                var state = workflowDefinition.States[index];
                var actionLabels = GenerateActionLabels(state, index);
                foreach (var kvp in actionLabels)
                {
                    labels.Add(kvp.Key, kvp.Value);
                }
                
            }

            for (var index = 0; index < workflowDefinition.States.Count; index++)
            {
                var state = workflowDefinition.States[index];
                GenerateStateDetails(sb, state, index);
                
            }
            
        }

        var indexOfInitialState = workflowDefinition.States.FindIndex(s => s.Name == workflowDefinition.InitialState);
        sb.AppendLine($"\n[*] --> state.{indexOfInitialState}");

        for (var index = 0; index < workflowDefinition.States.Count; index++)
        {
            var state = workflowDefinition.States[index];
            var name = labels[$"state.{index}"];
            foreach (var transition in state.Transitions)
            {
                var condition = IsDelayAction(workflowDefinition, state.Name)
                    ? GetDelayCondition(workflowDefinition, state.Name)
                    : IsScheduleAction(workflowDefinition, state.Name)
                        ? GetScheduleCondition(workflowDefinition, state.Name)
                        : transition.Condition;
                var indexOfNextState = workflowDefinition.States.FindIndex(s => s.Name == transition.NextState);
                
                sb.AppendLine($"state.{index} --> state.{indexOfNextState} : {condition}");
            }

        }

        var indexOfEndState = workflowDefinition.States.FindIndex(s => s.Name == "End");
        sb.AppendLine($"state.{indexOfEndState} --> [*]\n");
        
        foreach (var kvp in labels)
        {
            sb.AppendLine($"{kvp.Key}: {kvp.Value}");
        }
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
                actionLabels.Add($"OnEnterAction.{stateIndex}.{actionIndex}.{parameter.Key.Sanitize()}",
                    parameter.Key.Sanitize() + "#58;#32;" + parameter.Value?.ToString()?.Sanitize());
            }
            foreach (var parameter in stateDefinition.OnExitActions[actionIndex].Parameters)
            {
                actionLabels.Add($"OnExitAction.{stateIndex}.{actionIndex}.{parameter.Key.Sanitize()}", 
                    parameter.Key.Sanitize() + "#58;#32;" + parameter.Value?.ToString()?.Sanitize());
            }
            actionLabels.Add($"Assignments.{stateIndex}.{actionIndex}.Groups", 
                "Groups#58;#32;" + string.Join(", ", stateDefinition.Assignments.Groups));
            actionLabels.Add($"Assignments.{stateIndex}.{actionIndex}.Users", 
                "Users#58;#32;" + string.Join(", ", stateDefinition.Assignments.Users));
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
    
    private static void GenerateStateDetails(StringBuilder sb, StateDefinition state, int stateIndex)
    {
        
        sb.AppendLine($"state state.{stateIndex} {{");
        var onEnterLabel = $"OnEnter.{Guid.NewGuid():N}";
        sb.AppendLine(onEnterLabel + ": OnEnter");
        var onExitLabel = $"OnExit.{Guid.NewGuid():N}";
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