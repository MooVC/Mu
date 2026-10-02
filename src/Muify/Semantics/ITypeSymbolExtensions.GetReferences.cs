namespace Muify.Semantics
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        public static ImmutableArray<Poco> GetReferences(this ITypeSymbol symbol)
        {
            return symbol
                .GetReferencedTypes()
                .Select(type => Poco.Undefined
                    .Enumerate((property, poco) => poco.WithAttributes(attribute => attribute.From(property)), type.GetProperties())
                    .HasBinder(type is INamedTypeSymbol named && named.HasBinder())
                    .IsPartial(type.IsPartial() && SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, symbol.ContainingAssembly))
                    .WithCharacteristics(type.GetCharacteristics())
                    .WithQualification(type.ToQualification()))
                .ToImmutableArray();
        }
    }
}