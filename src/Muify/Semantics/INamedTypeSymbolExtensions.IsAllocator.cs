namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string AllocatorMetadataName = "IAllocator`1";

        public static bool IsAllocator(this INamedTypeSymbol type, ITypeSymbol identityType)
        {
            if (!IsAllocatorInterfaceType(type))
            {
                return false;
            }

            return identityType is null
                ? Unit.Undefined.Identity.Equals(type.TypeArguments[0].ToQualification())
                : SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], identityType);
        }

        private static bool IsAllocatorInterfaceType(INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == AllocatorMetadataName
                && definition.ContainingNamespace.ToDisplayString() == ServicesNamespace
                && type.TypeArguments.Length != 0;
        }
    }
}