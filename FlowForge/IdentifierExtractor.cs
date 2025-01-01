using Esprima;
using Esprima.Ast;
using Esprima.Utils;

namespace FlowForge;

public class IdentifierExtractor : AstVisitor
{
    private readonly Stack<Node> _parentNodes = new();
    public HashSet<string> TopLevelIdentifiers { get; } = new();

    public override object? Visit(Node node)
    {
        _parentNodes.Push(node); // Track the parent node
        var result = base.Visit(node);
        _parentNodes.Pop(); // Remove the current node after visiting
        return result;
    }
    protected override object? VisitIdentifier(Identifier identifier)
    {
        var parentNode = _parentNodes.ToArray()[1];
        // Check if the identifier is part of a MemberExpression as a property
        if (parentNode is MemberExpression member && member.Property == identifier)
        {
            // Skip member properties like "state" in "userState.state"
            return base.VisitIdentifier(identifier);
        }

        // Add only top-level identifiers
        TopLevelIdentifiers.Add(identifier.Name);
        return base.VisitIdentifier(identifier);
    }
    
    public static IEnumerable<string> DetectIdentifiers(string script)
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(script);

        var extractor = new IdentifierExtractor();
        extractor.Visit(program);

        return extractor.TopLevelIdentifiers;
    }
}