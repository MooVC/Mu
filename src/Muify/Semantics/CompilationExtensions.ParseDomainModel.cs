namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using Mu.Modelling;
    using static Muify.Semantics.INamedTypeSymbolExtensions;

    internal static partial class CompilationExtensions
    {
        public static Model ParseDomainModel(this Compilation compilation, Feature feature, (Name Area, Name Unit) names, CancellationToken cancellationToken, string domainNamespace = null)
        {
            if (domainNamespace is null)
            {
                string projectNamespace = compilation.AssemblyName ?? string.Empty;
                int separator = projectNamespace.LastIndexOf('.');
                domainNamespace = feature.IsUndefined || separator < 0 ? projectNamespace : projectNamespace.Substring(0, separator);
            }

            INamedTypeSymbol definition = compilation.GetTypeByMetadataName($"{domainNamespace}.{names.Unit}");

            if (definition.GetProjectKind() != ProjectKind.Unit)
            {
                return Model.Undefined;
            }

            definition.IdentifyMembers(out Component[] components, out List[] lists, cancellationToken);

            IOrderedEnumerable<IPropertySymbol> attributes = definition
                .GetProperties(IsAggregateStateProperty)
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
                            .WithAllocator(definition.Allocator(compilation.Assembly)))
                        .Enumerate((defined, subject) => subject.Featuring(defined), feature.IsUndefined ? Array.Empty<Feature>() : new[] { feature })
                        .Owns(components)
                        .Sets(lists)));
        }

        private static bool IsAggregateStateProperty(IPropertySymbol property)
        {
            return property.DeclaredAccessibility == Accessibility.Public
                && !property.IsStatic
                && !property.IsIndexer
                && property.SetMethod?.DeclaredAccessibility == Accessibility.Public;
        }
    }
}