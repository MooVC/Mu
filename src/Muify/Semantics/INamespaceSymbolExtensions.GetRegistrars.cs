namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamespaceSymbolExtensions
    {
        public static ImmutableArray<Qualification> GetRegistrars(this INamespaceSymbol @namespace, bool includeDescendants = false)
        {
            return (includeDescendants ? @namespace.GetAllTypes() : @namespace.GetTypeMembers())
                .Where(type => type.TypeKind == TypeKind.Class && !type.IsGenericType
                    && type.AllInterfaces.Any(@interface => @interface.IsRegistrar()))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .Distinct()
                .ToImmutableArray();
        }
    }
}