namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Qualification GetRoot(this INamedTypeSymbol request, ITypeSymbol aggregate)
        {
            INamedTypeSymbol implementation = request.ContainingNamespace.GetAllTypes()
                .Where(IsConcreteServiceType)
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface =>
                    IsRootInterfaceForRequest(@interface, request, aggregate)));

            return implementation is null ? Qualification.Unnamed : implementation.ToQualification();
        }

        private static bool IsRootInterfaceForRequest(INamedTypeSymbol @interface, INamedTypeSymbol request, ITypeSymbol aggregate)
        {
            return @interface.OriginalDefinition.MetadataName == "IRoot`2"
                && @interface.ContainingNamespace.ToDisplayString() == "Mu.Modelling.Services"
                && (aggregate is null || SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], aggregate))
                && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[1], request);
        }
    }
}