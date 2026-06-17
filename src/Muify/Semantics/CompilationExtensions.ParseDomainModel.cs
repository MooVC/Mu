namespace Muify.Semantics
{
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
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
                            .Enumerate(
                                (registrar, subject) => subject.WithRegistrars(registrar),
                                definition.ContainingNamespace.GetRegistrars()))
                        .Featuring(feature)
                        .Owns(components)
                        .Sets(lists)));
        }

        private static Qualification GetUnitIdentity(this INamedTypeSymbol definition)
        {
            AttributeData match = definition
                .GetAttributes()
                .FirstOrDefault(attribute => attribute.AttributeClass.IsUnitAttribute());

            INamedTypeSymbol unitType = match?.AttributeClass;

            if (unitType is null || unitType.TypeArguments.Length == 0)
            {
                return Qualification.Unnamed;
            }

            return unitType.TypeArguments[0].ToQualification();
        }

        private static Qualification ToQualification(this ITypeSymbol type)
        {
            if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
            {
                return type.Name;
            }

            return (Name: type.Name, Qualifier: type.ContainingNamespace.ToDisplayString());
        }
    }
}