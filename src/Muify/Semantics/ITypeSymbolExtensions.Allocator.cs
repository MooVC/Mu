namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static Service Allocator(this ITypeSymbol definition, IAssemblySymbol assembly)
        {
            ITypeSymbol identity = definition.GetUnitIdentityType();

            IEnumerable<INamedTypeSymbol> types = definition.ContainingAssembly.GlobalNamespace.GetAllTypes();

            if (!SymbolEqualityComparer.Default.Equals(definition.ContainingAssembly, assembly))
            {
                types = types.Concat(assembly.GlobalNamespace.GetAllTypes());
            }

            INamedTypeSymbol allocator = types
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