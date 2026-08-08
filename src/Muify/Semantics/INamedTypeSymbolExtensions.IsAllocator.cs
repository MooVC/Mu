namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string AllocatorMetadataName = "IAllocator`1";

        public static bool IsAllocator(this INamedTypeSymbol type, ITypeSymbol identityType)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            if (definition.MetadataName != AllocatorMetadataName
             || definition.ContainingNamespace.ToDisplayString() != ServicesNamespace
             || type.TypeArguments.Length == 0)
            {
                return false;
            }

            return identityType is null
                ? Unit.Undefined.Identity.Equals(type.TypeArguments[0].ToQualification())
                : SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], identityType);
        }
    }
}