namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static Qualification GetUnitIdentity(this INamedTypeSymbol definition)
        {
            AttributeData match = definition
                .GetAttributes()
                .FirstOrDefault(attribute => attribute.AttributeClass.IsUnitAttribute());

            INamedTypeSymbol unitType = match?.AttributeClass;

            if (unitType is null || unitType.TypeArguments.Length == 0)
            {
                return Qualification.Unnamed;
            }

            return unitType.TypeArguments[0].ToQualification();
        }
    }
}