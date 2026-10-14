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

            if (!RequiresConstructorConstraint(context.Compilation, type) || SatisfiesConstructorConstraint(type))
            {
                return;
            }

            foreach (SyntaxReference reference in type.DeclaringSyntaxReferences)
            {
                if (IsPositionalRecord(reference.GetSyntax(context.CancellationToken), out RecordDeclarationSyntax declaration))
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

        private static bool IsFeature(Compilation compilation, INamedTypeSymbol type)
        {
            if (type.HasUseCaseBase())
            {
                return true;
            }

            INamedTypeSymbol nonMutationalAttribute = compilation.GetTypeByMetadataName("Muify.Service.NonMutationalAttribute");

            return type.GetAttributes().Any(attribute => IsFeatureAttribute(attribute, nonMutationalAttribute));
        }

        private static bool IsFeatureAttribute(AttributeData attribute, INamedTypeSymbol nonMutationalAttribute)
        {
            return attribute.AttributeClass.IsMutationalAttribute()
                || (nonMutationalAttribute is object
                    && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, nonMutationalAttribute));
        }

        private static bool IsPositionalRecord(SyntaxNode syntax, out RecordDeclarationSyntax declaration)
        {
            declaration = syntax as RecordDeclarationSyntax;

            return declaration?.ParameterList?.Parameters.Count > 0;
        }

        private static bool RequiresConstructorConstraint(Compilation compilation, INamedTypeSymbol type)
        {
            INamedTypeSymbol unitAttribute = compilation.GetTypeByMetadataName("Muify.Domain.UnitAttribute`1");

            bool isUnit = unitAttribute is object && type
                .GetAttributes()
                .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass?.OriginalDefinition, unitAttribute));

            return type.HasAggregateBase() || isUnit || IsFeature(compilation, type);
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