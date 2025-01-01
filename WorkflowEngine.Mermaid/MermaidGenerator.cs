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
            GeneratorAllActionLabels(workflowDefinition, labels);
            AppendDetailedStateInfo(workflowDefinition, labels, sb);
        }

        AppendStateTransitions(workflowDefinition, sb, labels);

        AppendEndState(workflowDefinition, sb);

        AppendStateLabels(sb, labels);
        return sb.ToString();
    }

    private static void AppendDetailedStateInfo(WorkflowDefinition workflowDefinition, Dictionary<string, string> labels, StringBuilder sb)
    {
        for (var index = 0; index < workflowDefinition.States.Count; index++)
        {
            var state = workflowDefinition.States[index];
            GenerateStateDetails(sb, state, index);
        }
    }

    private static void GeneratorAllActionLabels(WorkflowDefinition workflowDefinition, Dictionary<string, string> labels)
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
    }

    private static void AppendStateTransitions(WorkflowDefinition workflowDefinition, StringBuilder sb, Dictionary<string, string> labels)
    {
        var indexOfInitialState = workflowDefinition.States.FindIndex(s => s.Name == workflowDefinition.InitialState);
        sb.AppendLine($"\n[*] --> StateLbl_{indexOfInitialState}");

        for (var index = 0; index < workflowDefinition.States.Count; index++)
        {
            var state = workflowDefinition.States[index];
            foreach (var transition in state.Transitions)
            {
                var condition = GetTransitionCondition(workflowDefinition, state.Name, transition.Condition);
                var indexOfNextState = workflowDefinition.States.FindIndex(s => s.Name == transition.NextState);
                sb.AppendLine($"StateLbl_{index} --> StateLbl_{indexOfNextState} : {condition}");
            }

        }
    }

    private static string GetTransitionCondition(WorkflowDefinition workflowDefinition, string stateName, string defaultCondition)
    {
        return IsDelayAction(workflowDefinition, stateName)
            ? GetDelayCondition(workflowDefinition, stateName)
            : IsScheduleAction(workflowDefinition, stateName)
                ? GetScheduleCondition(workflowDefinition, stateName)
                : defaultCondition;
    }

    private static void AppendEndState(WorkflowDefinition workflowDefinition, StringBuilder sb)
    {
        var indexOfEndState = workflowDefinition.States.FindIndex(s => s.Name == "End");
        sb.AppendLine($"StateLbl_{indexOfEndState} --> [*]\n");
    }

    private static void AppendStateLabels(StringBuilder sb, Dictionary<string, string> labels)
    {
        var diagramBeforeAddingLabels = sb.ToString();
        foreach (var kvp in labels.Where(kvp => diagramBeforeAddingLabels.Contains(kvp.Key)))
        {
            sb.AppendLine($"{kvp.Key}: {kvp.Value}");
        }
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
            var nameLabel = $"StateLbl_{index}";
            stateNameLabels.Add(nameLabel, name);
        }
        return stateNameLabels;
    }

    private static Dictionary<string, string> GenerateActionLabels(StateDefinition state, int stateIndex)
    {
        var actionLabels = new Dictionary<string, string>()
        {
            [$"OnEnter_{stateIndex}"] = "OnEnter",
            [$"OnExit_{stateIndex}"] = "OnExit",
            [$"Assignments_{stateIndex}"] = "Assignments",
            [$"Assignments_{stateIndex}_Groups"] = $"Groups#58;#32;{string.Join(",#32;", state.Assignments.Groups.Select(g => g.Sanitize()))}",
            [$"Assignments_{stateIndex}_Users"] = $"Users#58;#32;{string.Join(",#32;", state.Assignments.Users.Select(u => u.Sanitize()))}"
        };
        AppendActionDetailLabels(actionLabels, state.OnEnterActions, $"OnEnterAction_{stateIndex}");
        AppendActionDetailLabels(actionLabels, state.OnExitActions, $"OnExitAction_{stateIndex}");
        return actionLabels;
    }

    private static void AppendActionDetailLabels(Dictionary<string, string> labels, List<WorkflowAction> actions,
        string baseLabel)
    {
        for (var actionIndex = 0; actionIndex < actions.Count; actionIndex++)
        {
            foreach (var parameter in actions[actionIndex].Parameters)
            {
                labels.Add($"{baseLabel}_{actionIndex}_{parameter.Key.Sanitize()}",
                    parameter.Key.Sanitize() + "#58;#32;" + parameter.Value?.ToString()?.Sanitize());
            }
        }
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
        
        sb.AppendLine($"state StateLbl_{stateIndex} {{");
        AppendAssignments(sb, state, stateIndex);

        var previousNode = "[*]";
        previousNode = AppendStateActions(sb, "OnEnter", state.OnEnterActions, stateIndex, previousNode);
        previousNode = AppendStateActions(sb, "OnExit", state.OnExitActions, stateIndex, previousNode);

        sb.AppendLine($$"""
                       {{previousNode}} --> [*]
                       }
                       """);
    }

    private static string AppendStateActions(StringBuilder sb, string actionType, List<WorkflowAction> actions, int stateIndex, string previousNode)
    {
        if (!(actions?.Count > 0)) return previousNode;
        
        sb.AppendLine($$"""
                        {{previousNode}} --> {{actionType}}_{{stateIndex}}
                        state {{actionType}}_{{stateIndex}} {
                            direction LR
                        """);

        for (var index = 0; index < actions.Count; index++)
        {
            var action = actions[index];
            sb.AppendLine($"            {FormatAction(action, $"{actionType}Action_{stateIndex}_{index}")}");
        }
        sb.AppendLine("        }");
        previousNode = $"{actionType}_{stateIndex}";
        return previousNode;
    }

    private static void AppendAssignments(StringBuilder sb, StateDefinition state, int stateIndex)
    {
        var useSeparator = false;
        // Add Assignments
        if (state.Assignments.Groups.Count > 0 || state.Assignments.Users.Count > 0)
        {
            sb.AppendLine($$"""
                            Assignments_{{stateIndex}}
                            state Assignments_{{stateIndex}} {
                                direction LR
                            """);
            if (state.Assignments.Groups.Count > 0)
                sb.AppendLine($"Assignments_{stateIndex}_Groups");
            if (state.Assignments.Users.Count > 0)
                sb.AppendLine($"Assignments_{stateIndex}_Users");
            sb.AppendLine("}");
            useSeparator = true;
        }

        if (useSeparator)
            sb.AppendLine("--");
    }

    private static string FormatAction(WorkflowAction action, string label)
    {
        
        return string.Join("\n", action.Parameters.Select(kvp => $"{label}_" +kvp.Key));
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