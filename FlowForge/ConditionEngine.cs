using System.Text.Json;
using FlowForge.Enums;
using Jint;

namespace FlowForge;

internal class ConditionEngine(
    IEventLogger eventLogger,
    IDataProvider dataProvider,
    VariableUrlMappings variableUrlMappings): IConditionEngine
{
    public async Task<bool> EvaluateCondition(string condition, WorkflowInstance? instance, string actingState, string? eventName)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty.");

        var jintInterpreter = new Engine();

        if (instance is not null)
        {
            // register workflow data
            foreach (var kvp in instance.WorkflowData)
            {
                jintInterpreter.SetValue(kvp.Key, kvp.Value);
            }
            
            // Register variables from the state data
            foreach (var kvp in instance.StateData)
            {
                jintInterpreter.SetValue(kvp.Key, kvp.Value);
            }
        }

        // always set system variables after state data so they don't get overriden
        jintInterpreter.SetValue("event", eventName);
        
        // lastly, try to fetch variables via data providers
        var extractedIdentifiers = IdentifierExtractor.DetectIdentifiers(condition);
        var unknownIdentifiers = extractedIdentifiers
            .Where(id => jintInterpreter.GetValue(id).IsUndefined())
            .ToList();
        var fetchedVariables = await FetchMissingVariablesAsync(
            unknownIdentifiers, 
            instance?.Id ?? new WorkflowInstanceId(Guid.Empty), 
            instance?.StateData ?? new Dictionary<string, object>());
        foreach (var variable in fetchedVariables)
        {
            var parsedValue = ParseJsonValue(variable.Value.ToString() ?? string.Empty);
            jintInterpreter.SetValue(variable.Key, parsedValue);
        }
        

        try
        {
            // Evaluate the condition
            var result = jintInterpreter.Evaluate(condition);
            return Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            // Log and handle errors
            await eventLogger.LogEventAsync(StandardEvents.ConditionEvalFailure.ToString(), instance?.Id, instance?.DefinitionId,
                $"Condition evaluation failed: {condition}. Error: {ex.Message}", 
                actingState, instance?.ActiveStates);
            throw;
        }
    }
    
    private object ParseJsonValue(string jsonString)
    {
        if (!IsJson(jsonString))
            throw new JsonException($"JSON string is not a valid JSON. Received: {jsonString}");
        
        using var document = JsonDocument.Parse(jsonString);
        return document.ConvertJsonToDictionary();
    }
    
    private async Task<Dictionary<string, object>> FetchMissingVariablesAsync(
        IEnumerable<string> variables,
        WorkflowInstanceId instanceId,
        Dictionary<string, object> stateData)
    {
        var fetchedData = new Dictionary<string, object>();

        foreach (var variable in variables)
        {
            // Skip variables already in the state data
            if (stateData.ContainsKey(variable))
                continue;

            // Query data providers for the missing variable
            var url = variableUrlMappings.GetUrl(variable);
            if (url is null)
            {
                throw new InvalidOperationException($"No URL mapping found for variable '{variable}'.");
            }
            
            var value = await dataProvider.GetDataAsync(url, instanceId, stateData);
            
            fetchedData[variable] = value;
        }

        return fetchedData;
    }
    
    private bool IsJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        // Check if the string starts and ends with JSON-specific characters
        if ((input.StartsWith("{") && input.EndsWith("}")) || // JSON object
            (input.StartsWith("[") && input.EndsWith("]")) || // JSON array
            (input.StartsWith("\"") && input.EndsWith("\"")) || // JSON string
            input.Equals("null", StringComparison.OrdinalIgnoreCase) || // JSON null
            input.Equals("true", StringComparison.OrdinalIgnoreCase) || // JSON boolean
            input.Equals("false", StringComparison.OrdinalIgnoreCase)) // JSON boolean
        {
            try
            {
                JsonDocument.Parse(input);
                return true; // Successfully parsed, it's JSON
            }
            catch
            {
                // Failed to parse; it's not valid JSON
                return false;
            }
        }

        return false; // Not JSON
    }

}