using System.Text.Json;

namespace WorkflowEngine.Core;

public static class JsonExtensions
{
    public static Dictionary<string, object> ConvertJsonElements(this Dictionary<string, object>? src)
    {
        return src.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value is JsonElement jsonElement ? ConvertJsonElement(jsonElement) : kvp.Value);
    }

    public static Dictionary<string, object> ConvertJsonToDictionary(this JsonDocument jsonDocument)
    {
        return (Dictionary<string, object>)ConvertJsonElement(jsonDocument.RootElement);
    }
    
    private static object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longVal) ? (object)longVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(prop => prop.Name, prop => ConvertJsonElement(prop.Value)),
            _ => throw new InvalidOperationException($"Unsupported JsonElement ValueKind: {element.ValueKind}")
        };
    }
}