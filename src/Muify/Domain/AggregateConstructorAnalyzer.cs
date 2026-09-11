namespace Muify.Domain
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Muify.Semantics;
    using static Muify.Domain.AggregateConstructorAnalyzer_Resources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AggregateConstructorAnalyzer
        : DiagnosticAnalyzer
    {
        internal const string ConstructorConstraintNotSatisfiedId = "MUIFY05";

        internal static readonly DiagnosticDescriptor ConstructorConstraintNotSatisfiedRule = new DiagnosticDescriptor(
            ConstructorConstraintNotSatisfiedId,
            GetResourceString(nameof(ConstructorConstraintTitle)),
            GetResourceString(nameof(ConstructorConstraintMessage)),
            GetResourceString(nameof(DiagnosticCategory)),
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString(nameof(ConstructorConstraintDescription)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(ConstructorConstraintNotSatisfiedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (!type.IsRecord)
            {
                return;
            }

            INamedTypeSymbol unitAttribute = context.Compilation.GetTypeByMetadataName("Muify.Domain.UnitAttribute`1");

            bool isUnit = unitAttribute is object && type
                .GetAttributes()
                .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass?.OriginalDefinition, unitAttribute));

            if (!(type.HasAggregateBase() || isUnit) || SatisfiesConstructorConstraint(type))
            {
                return;
            }

            foreach (SyntaxReference reference in type.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax(context.CancellationToken) is RecordDeclarationSyntax declaration
                 && declaration.ParameterList?.Parameters.Count > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        ConstructorConstraintNotSatisfiedRule,
                        declaration.Identifier.GetLocation(),
                        type.Name));

                    return;
                }
            }
        }

        private static string GetResourceString(string name)
        {
            return ResourceManager.GetString(name) ?? name;
        }

        private static bool HasRequiredMembers(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol current = type; current is object; current = current.BaseType)
            {
                if (current.GetMembers().Any(IsRequiredMember))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRequiredMember(ISymbol member)
        {
            return (member is IPropertySymbol property && property.IsRequired)
                || (member is IFieldSymbol field && field.IsRequired);
        }

        private static bool SatisfiesConstructorConstraint(INamedTypeSymbol type)
        {
            if (type.IsAbstract)
            {
                return false;
            }

            IMethodSymbol constructor = type.InstanceConstructors.FirstOrDefault(IsPublicDefaultConstructor);

            return constructor is object && (!HasRequiredMembers(type) || SetsRequiredMembers(constructor));
        }

        private static bool IsPublicDefaultConstructor(IMethodSymbol candidate)
        {
            return candidate.DeclaredAccessibility == Accessibility.Public && candidate.Parameters.Length == 0;
        }

        private static bool SetsRequiredMembers(IMethodSymbol constructor)
        {
            return constructor
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
        }
    }
}