namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static Qualification Allocator(this INamedTypeSymbol definition)
        {
            ITypeSymbol identityType = definition.GetUnitIdentityType();

            INamedTypeSymbol allocator = definition.ContainingAssembly.GlobalNamespace
                .GetAllTypes()
                .Where(type => type.TypeKind == TypeKind.Class)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface => @interface.IsAllocator(identityType)));

            if (allocator is object)
            {
                return allocator.ToQualification();
            }

            return Qualification.Unnamed;
        }
    }
}