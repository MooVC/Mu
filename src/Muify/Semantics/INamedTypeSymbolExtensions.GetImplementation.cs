namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Qualification GetImplementation(this INamedTypeSymbol request, string metadataName, string qualifier)
        {
            INamedTypeSymbol implementation = request.ContainingNamespace.GetAllTypes()
                .Where(IsConcreteServiceType)
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface =>
                    IsRequestedServiceInterface(@interface, request, metadataName, qualifier)));

            return implementation is null ? Qualification.Unnamed : implementation.ToQualification();
        }

        private static bool IsConcreteServiceType(INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Class && !type.IsAbstract && !type.IsGenericType;
        }

        private static bool IsRequestedServiceInterface(INamedTypeSymbol @interface, INamedTypeSymbol request, string metadataName, string qualifier)
        {
            return @interface.OriginalDefinition.MetadataName == metadataName
                && @interface.ContainingNamespace.ToDisplayString() == qualifier
                && @interface.TypeArguments.Length == 2
                && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], request);
        }
    }
}