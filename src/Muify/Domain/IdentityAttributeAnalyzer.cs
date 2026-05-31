namespace Muify.Domain
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Muify.Semantics;
    using static Muify.Domain.IdentityAttributeAnalyzer_Resources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IdentityAttributeAnalyzer
        : DiagnosticAnalyzer
    {
        internal const string DuplicateIdentityAttributeId = "MUIFY02";
        internal const string TypeNotSupportedId = "MUIFY01";

        internal static readonly DiagnosticDescriptor DuplicateIdentityAttributeRule = new DiagnosticDescriptor(
            DuplicateIdentityAttributeId,
            GetResourceString(nameof(DuplicateIdentityAttributeTitle)),
            GetResourceString(nameof(DuplicateIdentityAttributeMessage)),
            GetResourceString(nameof(DiagnosticCategory)),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString(nameof(DuplicateIdentityAttributeDescription)));

        internal static readonly DiagnosticDescriptor TypeNotSupportedRule = new DiagnosticDescriptor(
            TypeNotSupportedId,
            GetResourceString(nameof(TypeNotSupportedTitle)),
            GetResourceString(nameof(TypeNotSupportedMessage)),
            GetResourceString(nameof(DiagnosticCategory)),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString(nameof(TypeNotSupportedDescription)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(TypeNotSupportedRule, DuplicateIdentityAttributeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (!(type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct))
            {
                return;
            }

            ReportInvalidTypeAttribute(context, type);

            var identityProperties = new List<IPropertySymbol>();

            foreach (ISymbol member in type.GetMembers())
            {
                if (member.Kind == SymbolKind.NamedType)
                {
                    continue;
                }

                AnalyzeMember(context, type, member, identityProperties);
            }

            ReportDuplicateIdentityAttributes(context, type, identityProperties);
        }

        private static void AnalyzeMember(SymbolAnalysisContext context, INamedTypeSymbol type, ISymbol member, ICollection<IPropertySymbol> properties)
        {
            bool isIdentityProperty = false;

            IEnumerable<AttributeData> attributes = member
                .GetAttributes()
                .Where(attribute => attribute.AttributeClass.IsIdentityAttribute());

            foreach (AttributeData attribute in attributes)
            {
                if (!(member is IPropertySymbol property))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypeNotSupportedRule,
                        GetLocation(attribute, member, context.CancellationToken)));

                    continue;
                }

                isIdentityProperty = true;

                if (!IsSupportedType(type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        TypeNotSupportedRule,
                        GetLocation(attribute, property, context.CancellationToken)));
                }
            }

            if (isIdentityProperty)
            {
                properties.Add((IPropertySymbol)member);
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
            return IdentityAttributeAnalyzer_Resources.ResourceManager.GetString(name) ?? name;
        }

        private static bool IsSupportedType(INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Class && !type.IsRecord;
        }

        private static void ReportDuplicateIdentityAttributes(SymbolAnalysisContext context, INamedTypeSymbol type, ICollection<IPropertySymbol> properties)
        {
            if (properties.Count <= 1)
            {
                return;
            }

            foreach (IPropertySymbol property in properties)
            {
                AttributeData attribute = property
                    .GetAttributes()
                    .First(match => match.AttributeClass.IsIdentityAttribute());

                context.ReportDiagnostic(Diagnostic.Create(
                    DuplicateIdentityAttributeRule,
                    GetLocation(attribute, property, context.CancellationToken),
                    type.Name));
            }
        }

        private static void ReportInvalidTypeAttribute(SymbolAnalysisContext context, INamedTypeSymbol type)
        {
            foreach (AttributeData attribute in type.GetAttributes().Where(attribute => attribute.AttributeClass.IsIdentityAttribute()))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeNotSupportedRule,
                    GetLocation(attribute, type, context.CancellationToken)));
            }
        }
    }
}