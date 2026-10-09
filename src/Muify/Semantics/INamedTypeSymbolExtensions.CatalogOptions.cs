namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Poco CatalogOptions(this INamedTypeSymbol options)
        {
            return options is null
                ? Poco.Undefined
                : Poco.Undefined
                    .HasBase(options.BaseType is object && options.BaseType.SpecialType != SpecialType.System_Object)
                    .IsPartial(options.IsPartial())
                    .WithCharacteristics(options.GetCharacteristics())
                    .WithQualification(options.ToQualification());
        }
    }
}