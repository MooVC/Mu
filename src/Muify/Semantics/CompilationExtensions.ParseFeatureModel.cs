namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Modelling;
    using ModellingResult = Mu.Modelling.Result;

    internal static partial class CompilationExtensions
    {
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
                .Enumerate(CreateResult, results);
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
    }
}