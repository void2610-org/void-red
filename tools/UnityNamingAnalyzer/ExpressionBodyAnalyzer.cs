using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityNamingAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ExpressionBodyAnalyzer : DiagnosticAnalyzer
    {
        // 単一文のpublicメソッドには式本体を使用するよう警告
        public static readonly DiagnosticDescriptor UNA0004 = new DiagnosticDescriptor(
            "UNA0004",
            "単一文のpublicメソッドには式本体を使用してください",
            "メソッド '{0}' は単一文のため式本体 (=>) で記述してください",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(UNA0004);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            // 既に式本体の場合は除外
            if (method.ExpressionBody != null)
                return;

            // ブロック本体がない場合は除外（抽象メソッド等）
            if (method.Body == null)
                return;

            // publicメソッドのみ対象
            if (!method.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            // ステートメントが1つだけの場合に警告
            if (method.Body.Statements.Count != 1)
                return;

            // 式本体に変換可能な文のみ対象（return文、式文のみ）
            var statement = method.Body.Statements[0];
            if (!(statement is ReturnStatementSyntax) && !(statement is ExpressionStatementSyntax))
                return;

            var diagnostic = Diagnostic.Create(
                UNA0004,
                method.Identifier.GetLocation(),
                method.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
