namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        public static Qualification GetUnitIdentity(this ITypeSymbol definition)
        {
            ITypeSymbol identityType = definition.GetUnitIdentityType();

            if (identityType is null)
            {
                return Qualification.Unnamed;
            }

            return identityType.ToQualification();
        }

        public static ITypeSymbol GetUnitIdentityType(this ITypeSymbol definition)
        {
            AttributeData match = definition
                .GetAttributes()
                .FirstOrDefault(attribute => attribute.AttributeClass != null
                    && attribute.AttributeClass.IsUnitAttribute());

            INamedTypeSymbol unitType = match?.AttributeClass;

            if (unitType is null || unitType.TypeArguments.Length == 0)
            {
                return default;
            }

            return unitType.TypeArguments[0];
        }
    }
}