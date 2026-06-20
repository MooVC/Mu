namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static bool HasAllocator(this INamedTypeSymbol definition)
        {
            ITypeSymbol identityType = definition.GetUnitIdentityType();

            Qualification identity = identityType is null
                ? Qualification.Unnamed
                : identityType.ToQualification();

            if (!identity.IsUnnamed && !Unit.Undefined.Identity.Equals(identity))
            {
                return false;
            }

            return definition.ContainingAssembly.GlobalNamespace
                .GetAllTypes()
                .Any(type => type.TypeKind == TypeKind.Class
                    && type.AllInterfaces.Any(@interface => @interface.IsAllocator(identity, identityType)));
        }
    }
}