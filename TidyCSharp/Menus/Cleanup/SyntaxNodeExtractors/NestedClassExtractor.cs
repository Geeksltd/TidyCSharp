using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class NestedClassExtractor : ISyntaxTreeMemberExtractor
    {
        public List<SyntaxToken> Extraxt(SyntaxNode root, SyntaxKind kind)
        {
            return root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                                         .Where(cls => cls.Modifiers.Any(m => m.IsKind(kind)))
                                         .SelectMany(method => method.Modifiers.Where(m => m.IsKind(kind)))
                                         .ToList();
        }
    }
}