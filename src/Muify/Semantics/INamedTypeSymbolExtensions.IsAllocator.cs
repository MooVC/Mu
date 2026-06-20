namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string AllocatorMetadataName = "IAllocator`1";

        private static bool IsAllocator(this INamedTypeSymbol type, Qualification identity, ITypeSymbol identityType)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            if (definition.MetadataName != AllocatorMetadataName
             || definition.ContainingNamespace.ToDisplayString() != ServicesNamespace
             || type.TypeArguments.Length == 0)
            {
                return false;
            }

            return identity.IsUnnamed
                ? Unit.Undefined.Identity.Equals(type.TypeArguments[0].ToQualification())
                : SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], identityType);
        }
    }
}