namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        public static ImmutableArray<Poco> GetReferences(this ITypeSymbol symbol, ITypeSymbol aggregate = null)
        {
            var domainBindings = new HashSet<ITypeSymbol>(GetDomainBindings(aggregate, symbol), SymbolEqualityComparer.Default);

            return symbol
                .GetReferencedTypes()
                .Concat(symbol.GetResultTypes())
                .Where(type => !SymbolEqualityComparer.Default.Equals(type, symbol))
                .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
                .Select(type => CatalogReference(type, symbol, domainBindings))
                .ToImmutableArray();
        }

        private static Poco CatalogReference(ITypeSymbol type, ITypeSymbol request, ISet<ITypeSymbol> domainBindings)
        {
            INamedTypeSymbol[] containers = GetReferenceContainers(type).ToArray();

            return Poco.Undefined
                .Enumerate((property, poco) => poco.WithAttributes(attribute => attribute.From(property)), type.GetProperties(IsBindableReferenceProperty))
                .Enumerate(
                    (container, poco) => poco.WithContainingTypes(Poco.Undefined
                        .WithCharacteristics(container.GetCharacteristics())
                        .WithQualification(container.ToQualification())),
                    containers)
                .HasBinder(type is INamedTypeSymbol named && named.HasBinder())
                .HasDomainBinder(domainBindings.Contains(type))
                .IsAccessible(IsAccessibleReference(type) && containers.All(IsAccessibleReference))
                .IsPartial(CanExtendReference(type, request) && containers.All(container => CanExtendReference(container, request)))
                .WithCharacteristics(type.GetCharacteristics())
                .WithQualification(type.ToQualification());
        }

        private static bool CanExtendReference(ITypeSymbol type, ITypeSymbol request)
        {
            return type.IsPartial()
                && SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, request.ContainingAssembly);
        }

        private static bool CanUseDomainBinder(ITypeSymbol type, ITypeSymbol request)
        {
            return type is INamedTypeSymbol named
                && (named.TypeKind == TypeKind.Class || named.TypeKind == TypeKind.Struct)
                && (named.HasBinder() || CanExtendReference(type, request));
        }

        private static IEnumerable<ITypeSymbol> GetDomainBindings(ITypeSymbol aggregate, ITypeSymbol request)
        {
            if (aggregate is null)
            {
                yield break;
            }

            if (CanUseDomainBinder(aggregate, request))
            {
                yield return aggregate;
            }

            foreach (ITypeSymbol component in aggregate.GetReferencedTypes().Where(type => CanUseDomainBinder(type, request)))
            {
                yield return component;
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetReferenceContainers(ITypeSymbol type)
        {
            for (INamedTypeSymbol container = type.ContainingType; container is object; container = container.ContainingType)
            {
                yield return container;
            }
        }

        private static bool IsAccessibleReference(ITypeSymbol type)
        {
            return type is INamedTypeSymbol named
                && named.Arity == 0
                && !named.IsFileLocal
                && (named.DeclaredAccessibility == Accessibility.Public || named.DeclaredAccessibility == Accessibility.Internal);
        }

        private static bool IsBindableReferenceProperty(IPropertySymbol property)
        {
            return property.DeclaredAccessibility == Accessibility.Public
                && !property.IsStatic
                && !property.IsIndexer
                && property.SetMethod is object;
        }
    }
}