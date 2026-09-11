namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static Service Allocator(this ITypeSymbol definition)
        {
            ITypeSymbol identity = definition.GetUnitIdentityType();

            INamedTypeSymbol allocator = definition.ContainingAssembly.GlobalNamespace
                .GetAllTypes()
                .Where(type => type.TypeKind == TypeKind.Class)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface => @interface.IsAllocator(identity)));

            if (allocator is null)
            {
                return Service.Undefined;
            }

            return Service.Undefined
                .HasRegistrar(allocator.HasRegistrar())
                .IsPartial(allocator.IsPartial())
                .WithDefinition(allocator.ToQualification());
        }
    }
}