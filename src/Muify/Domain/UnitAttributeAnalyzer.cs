namespace Muify.Domain
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Muify.Semantics;
    using static Muify.Domain.UnitAttributeAnalyzer_Resources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnitAttributeAnalyzer
        : DiagnosticAnalyzer
    {
        internal const string TypeNameMismatchId = "MUIFY04";
        internal const string TypeNotSupportedId = "MUIFY03";

        internal static readonly DiagnosticDescriptor TypeNameMismatchRule = new DiagnosticDescriptor(
            TypeNameMismatchId,
            GetResourceString(nameof(TypeNameMismatchTitle)),
            GetResourceString(nameof(TypeNameMismatchMessage)),
            GetResourceString(nameof(DiagnosticCategory)),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString(nameof(TypeNameMismatchDescription)));

        internal static readonly DiagnosticDescriptor TypeNotSupportedRule = new DiagnosticDescriptor(
            TypeNotSupportedId,
            GetResourceString(nameof(TypeNotSupportedTitle)),
            GetResourceString(nameof(TypeNotSupportedMessage)),
            GetResourceString(nameof(DiagnosticCategory)),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString(nameof(TypeNotSupportedDescription)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(TypeNotSupportedRule, TypeNameMismatchRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (!(context.Symbol is INamedTypeSymbol type))
            {
                return;
            }

            IEnumerable<AttributeData> attributes = type
                .GetAttributes()
                .Where(attribute => attribute.AttributeClass.IsUnitAttribute());

            foreach (AttributeData attribute in attributes)
            {
                if (!type.IsRecord)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypeNotSupportedRule,
                        GetLocation(attribute, type, context.CancellationToken)));
                }

                if (!TypeNameMatchesNamespace(type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypeNameMismatchRule,
                        GetLocation(attribute, type, context.CancellationToken),
                        type.Name));
                }
            }
        }

        private static Location GetLocation(AttributeData attribute, ISymbol symbol, CancellationToken cancellationToken)
        {
            SyntaxReference reference = attribute.ApplicationSyntaxReference;

            if (reference is null)
            {
                return symbol.Locations.FirstOrDefault();
            }

            return reference.GetSyntax(cancellationToken).GetLocation();
        }

        private static string GetResourceString(string name)
        {
            return ResourceManager.GetString(name) ?? name;
        }

        private static bool TypeNameMatchesNamespace(INamedTypeSymbol type)
        {
            INamespaceSymbol @namespace = type.ContainingNamespace;

            return @namespace is object
                && !@namespace.IsGlobalNamespace
                && type.Name == @namespace.Name;
        }
    }
}