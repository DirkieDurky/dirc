using Dirc.Compiling.Parsing;

namespace Dirc.Compiling;

static class Helpers
{
    public static IEnumerable<AstNode> GetAllDescendantNodes(IEnumerable<AstNode> nodes)
    {
        var result = new List<AstNode>();
        foreach (var node in nodes)
        {
            result.Add(node);
            result.AddRange(GetAllDescendantNodes(node.GetChildNodes()));
        }
        return result;
    }
}
