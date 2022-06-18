using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Toendering.RoslynAnalyzer.Akka
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ToenderingRoslynAnalyzerAkkaPropsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TRD001";

        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string _category = "Usage";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);

        }

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocationExpression
                && invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(context.Node);
                ITypeSymbol type = typeInfo.Type;

                ITypeSymbol[] targetTypes = invocationExpression.ArgumentList.Arguments
                    .Select(x => context.SemanticModel.GetTypeInfo(x.Expression).Type)
                    .ToArray();

                if (type.ContainingAssembly.Identity.Name == "Akka" 
                    && identifier.Identifier.Text == "Props"
                    && memberAccess.Name is GenericNameSyntax genericName 
                    && genericName.TypeArgumentList.Arguments.Any())
                {

                    if (genericName.TypeArgumentList.Arguments.Count == 1)
                    {
                        TypeSyntax targetTypeSyntax = genericName.TypeArgumentList.Arguments[0];
                        INamedTypeSymbol candidateType = context.SemanticModel.GetTypeInfo(targetTypeSyntax).Type as INamedTypeSymbol;

                        bool hasMatchingCtor = candidateType.Constructors
                            .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                            .Any(method => MatchesMethodSignature(context.Compilation, method, targetTypes));

                        if (hasMatchingCtor)
                            return;

                        Diagnostic diagnostic = Diagnostic.Create(_rule,
                            invocationExpression.GetLocation(),
                            candidateType.Name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private bool MatchesMethodSignature(Compilation compilation, IMethodSymbol methodSymbol, IList<ITypeSymbol> args)
        {
            ITypeSymbol[] targetArgs = methodSymbol.Parameters.Select(x => x.Type).ToArray();
            if (args.Count != targetArgs.Length)
                return false;

            for (int i = 0; i < targetArgs.Length; i++)
            {
                ITypeSymbol sourceType = args[i];
                ITypeSymbol destinationType = targetArgs[i];

                if (sourceType.Equals(destinationType, SymbolEqualityComparer.Default))
                    continue;
                
                Conversion conversion = compilation.ClassifyConversion(sourceType, destinationType);
                if (conversion.Exists)
                    continue;

                return false;
            }

            return true;
        }
    }
}
