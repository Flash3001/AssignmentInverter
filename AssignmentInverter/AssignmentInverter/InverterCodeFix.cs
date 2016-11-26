using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AssignmentInverter.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace AssignmentInverter
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InverterCodeFix)), Shared]
    public class InverterCodeFix : CodeFixProvider
    {
        private const string title = "Invert Sides";

        public sealed override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(InverterAnalyzer.DiagnosticId);
        public sealed override FixAllProvider GetFixAllProvider() 
            => null;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var selectionSpan = EditorHelper.GetSelectionSpan();

            var selectedTokes = selectionSpan.Length == 0
                ? root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>()
                : root.DescendantNodes(selectionSpan).OfType<AssignmentExpressionSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => Invert(context.Document, selectedTokes, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> Invert(Document document, IEnumerable<AssignmentExpressionSyntax> assings, CancellationToken cancellationToken)
        {
            var originalSolution = document.Project.Solution;

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var editor = new SyntaxEditor(root, originalSolution.Workspace);

            foreach (var oldNode in assings)
            {
                editor.ReplaceNode(oldNode, InvertNode(oldNode));
            }
            
            return originalSolution.WithDocumentSyntaxRoot(document.Id, editor.GetChangedRoot());
        }

        private static AssignmentExpressionSyntax InvertNode(AssignmentExpressionSyntax oldNode)
        {
            var oldLeft = oldNode.Left;
            var oldRight = oldNode.Right;

            var triviaLeftLeading = oldLeft.GetLeadingTrivia();
            var triviaLeftTrailing = oldLeft.GetTrailingTrivia();
            var triviaRightLeading = oldRight.GetLeadingTrivia();
            var triviaRightTrailing = oldRight.GetTrailingTrivia();

            var newLeft = oldRight.WithLeadingTrivia(triviaLeftLeading).WithTrailingTrivia(triviaLeftTrailing);
            var newRight = oldLeft.WithLeadingTrivia(triviaRightLeading).WithTrailingTrivia(triviaRightTrailing);

            return oldNode.WithLeft(newLeft).WithRight(newRight);
        }
    }
}