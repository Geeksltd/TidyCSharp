using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
	public abstract class VariableRenamingBase : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		const string SELECTED_METHOD_ANNOTATION = "SELECTED_Node_To_RENAME_ANNOTATION";

		Document WorkingDocument, orginalDocument;

		public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
		{
			var annotationForSelectedNode = new SyntaxAnnotation(SELECTED_METHOD_ANNOTATION);
			orginalDocument = ProjectItemDetails.ProjectItemDocument;
			WorkingDocument = ProjectItemDetails.ProjectItemDocument;

			if (orginalDocument == null) return initialSourceNode;

			SyntaxNode workingNode;
			var annotatedRoot = initialSourceNode;
			do
			{
				workingNode = GetWorkingNode(annotatedRoot, annotationForSelectedNode);

				if (workingNode == null) continue;

				var annotatedNode = workingNode.WithAdditionalAnnotations(annotationForSelectedNode);
				annotatedRoot = annotatedRoot.ReplaceNode(workingNode, annotatedNode);
				WorkingDocument = WorkingDocument.WithSyntaxRoot(annotatedRoot);
				annotatedRoot = WorkingDocument.GetSyntaxRootAsync().Result;
				annotatedNode = annotatedRoot.GetAnnotatedNodes(annotationForSelectedNode).FirstOrDefault();

				var rewriter = GetRewriter(WorkingDocument);

				rewriter.Visit(annotatedNode);
				WorkingDocument = rewriter.WorkingDocument;
			} while (workingNode != null);

			return null;
		}

		protected override void SaveResult(SyntaxNode initialSourceNode)
		{
			if (string.Compare(WorkingDocument.GetTextAsync().Result.ToString(), ProjectItemDetails.InitialSourceNode.GetText().ToString(), false) != 0)
			{
				TidyCSharpPackage.Instance.RefreshSolution(WorkingDocument.Project.Solution);
			}
		}

		protected abstract SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes);

		protected abstract VariableRenamingBaseRewriter GetRewriter(Document workingDocument);

		protected abstract class VariableRenamingBaseRewriter : CleanupCSharpSyntaxRewriter
		{
			public Document WorkingDocument { get; protected set; }
			public VariableRenamingBaseRewriter(Document workingDocument, ICleanupOption options) : base(false, options)
			{
				WorkingDocument = workingDocument;
			}
		}
	}
}