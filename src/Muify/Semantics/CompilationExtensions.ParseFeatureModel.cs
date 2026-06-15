namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Modelling;
    using Muify.Service;
    using ModellingResult = Mu.Modelling.Result;

    internal static partial class CompilationExtensions
    {
        private const string BehaviorNamespace = "Mu.Modelling.Behavior";
        private const string ServicesNamespace = "Mu.Modelling.Services";
        private const string UseCaseName = "UseCase";

        public static Feature ParseFeatureModel(this Compilation compilation, (Name Area, Name Feature, Name Unit) names)
        {
            INamedTypeSymbol request = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{names.Feature}");

            if (request is null)
            {
                return Feature.Undefined;
            }

            IPropertySymbol[] results = request.GetResults();

            return Feature.Undefined
                .Named(names.Feature)
                .Enumerate(CreateResult, results)
                .WithMetadata(metadata => metadata
                    .HasBase(request.HasUseCaseBase())
                    .HasFact(request.HasFact())
                    .Enumerate((transform, subject) => subject.WithTransforms(transform), request.GetTransforms()));
        }

        private static Feature CreateResult(this IPropertySymbol result, Feature feature)
        {
            return feature.Returning(member => member
                .Named(result.Name)
                .OfType(result.Type));
        }

        private static IPropertySymbol[] GetResults(this INamedTypeSymbol request)
        {
            INamedTypeSymbol result = request
                .GetTypeMembers()
                .FirstOrDefault(type => type.IsRecord && type.Name == nameof(ModellingResult));

            if (result is null)
            {
                return Array.Empty<IPropertySymbol>();
            }

            return result
                .GetMembers()
                .OfType<IPropertySymbol>()
                .ToArray();
        }

        private static ImmutableArray<Qualification> GetTransforms(this INamedTypeSymbol request)
        {
            return request
                .ContainingNamespace
                .GetTypeMembers()
                .Where(type => type.TypeKind == TypeKind.Class
                    && type.AllInterfaces.Any(@interface => @interface.IsTransform()))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .ToImmutableArray();
        }

        private static bool HasFact(this INamedTypeSymbol request)
        {
            ITypeSymbol fact = request
                .GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .Where(attribute => attribute.IsMutationalAttribute())
                .Select(attribute => attribute.TypeArguments[0])
                .FirstOrDefault();

            return fact?.TypeKind == TypeKind.Class;
        }

        private static bool HasUseCaseBase(this INamedTypeSymbol request)
        {
            INamedTypeSymbol current = request.BaseType;

            while (current is object)
            {
                if (current.Name == UseCaseName
                    && current.ContainingNamespace.ToDisplayString() == BehaviorNamespace)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsMutationalAttribute(this INamedTypeSymbol attribute)
        {
            INamedTypeSymbol definition = attribute?.OriginalDefinition;

            return definition is object
                && definition.TypeArguments.Length == 1
                && definition.ContainingNamespace.ToDisplayString() == typeof(CreationalAttributeStrategy).Namespace
                && (definition.Name == $"{CreationalAttributeStrategy.Name}Attribute"
                 || definition.Name == $"{TransitionalAttributeStrategy.Name}Attribute");
        }

        private static bool IsTransform(this INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == "ITransform`2"
                && definition.ContainingNamespace.ToDisplayString() == ServicesNamespace;
        }
    }
}