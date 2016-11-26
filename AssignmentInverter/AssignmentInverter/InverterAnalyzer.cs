using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AssignmentInverter
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InverterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "InverterAnalyzer";
        internal static readonly LocalizableString Title = "Assignment Inverter";
        internal static readonly LocalizableString MessageFormat = "Invert selected assignments.";
        internal const string Category = "Refactor";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var assingNode = context.Node as AssignmentExpressionSyntax;
            var diagnostic = Diagnostic.Create(Rule, assingNode.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }
}