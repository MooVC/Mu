namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using Mu.Modelling;

    internal static partial class CompilationExtensions
    {
        public static Model ParseDomainModel(this Compilation compilation, Feature feature, (Name Area, Name Unit) names, CancellationToken cancellationToken)
        {
            const int DomainSegments = 4;

            string domainNamespace = string.Join(".", (compilation.AssemblyName ?? string.Empty).Split('.').Take(DomainSegments));
            INamedTypeSymbol definition = compilation.GetTypeByMetadataName($"{domainNamespace}.{names.Unit}");

            if (definition is null || !definition.IsRecord)
            {
                return Model.Undefined;
            }

            definition.IdentifyMembers(out Component[] components, out List[] lists, cancellationToken);

            IOrderedEnumerable<IPropertySymbol> attributes = definition
                .GetProperties(property => property.DeclaredAccessibility == Accessibility.Public
                    && !property.IsStatic
                    && !property.IsIndexer
                    && property.SetMethod?.DeclaredAccessibility == Accessibility.Public)
                .OrderBy(property => property.Name, StringComparer.Ordinal);

            return Model.Undefined
                .Defines(area => area
                    .Named(names.Area)
                    .ResponsibleFor(unit => unit
                        .Enumerate(
                            (property, subject) => subject.AttributedWith(attribute => attribute
                                .Named(property.Name)
                                .OfType(property.Type.ToSyntax())),
                            attributes)
                        .IdentifiedBy(definition.GetUnitIdentity())
                        .Named(names.Unit)
                        .WithMetadata(metadata => metadata
                            .Enumerate((registrar, subject) => subject.WithRegistrars(registrar), definition.ContainingNamespace.GetRegistrars())
                            .IsPartial(definition.IsPartial())
                            .HasBase(definition.HasAggregateBase())
                            .HasBinder(definition.HasBinder())
                            .HasRegistrar(definition.HasRegistrar())
                            .WithAllocator(definition.Allocator()))
                        .Featuring(feature)
                        .Owns(components)
                        .Sets(lists)));
        }
    }
}