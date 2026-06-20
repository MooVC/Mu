namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private static readonly Qualification _allocator = (Name: "GuidAllocator", Qualifier: "Mu.Modelling.Services");

        internal static Qualification Allocator(this INamedTypeSymbol definition)
        {
            ITypeSymbol identityType = definition.GetUnitIdentityType();
            Qualification identity = identityType is null
                ? Unit.Undefined.Identity
                : identityType.ToQualification();

            INamedTypeSymbol allocator = definition.ContainingAssembly.GlobalNamespace
                .GetAllTypes()
                .Where(type => type.TypeKind == TypeKind.Class)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface => @interface.IsAllocator(identityType)));

            if (allocator is object)
            {
                return allocator.ToQualification();
            }

            return Unit.Undefined.Identity.Equals(identity)
                ? _allocator
                : Qualification.Unnamed;
        }
    }
}