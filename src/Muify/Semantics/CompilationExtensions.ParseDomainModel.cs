namespace Muify.Semantics
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using Mu.Modelling;

    internal static partial class CompilationExtensions
    {
        public static Model ParseDomainModel(this Compilation compilation, Feature feature, (Name Area, Name Unit) names, CancellationToken cancellationToken)
        {
            INamedTypeSymbol definition = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{names.Unit}");

            if (definition is null || !definition.IsRecord)
            {
                return Model.Undefined;
            }

            definition.IdentifyMembers(out Component[] components, out List[] lists, cancellationToken);

            return Model.Undefined
                .Defines(area => area
                    .Named(names.Area)
                    .ResponsibleFor(unit => unit
                        .IdentifiedBy(definition.GetUnitIdentity())
                        .Named(names.Unit)
                        .WithMetadata(metadata => metadata
                            .HasAllocator(definition.HasAllocator())
                            .HasBase(definition.HasAggregateBase())
                            .Enumerate(
                                (registrar, subject) => subject.WithRegistrars(registrar),
                                definition.ContainingNamespace.GetRegistrars()))
                        .Featuring(feature)
                        .Owns(components)
                        .Sets(lists)));
        }
    }
}