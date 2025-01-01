namespace FlowForge;

public class VariableUrlMappings
{
    public Dictionary<string, string> Mappings = new();

    public void AddMapping(string variable, string url)
    {
        Mappings[variable] = url;
    }

    public string? GetUrl(string variable)
    {
        return Mappings.GetValueOrDefault(variable);
    }
}