namespace Muify.Semantics
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static IEnumerable<ITypeSymbol> GetResultTypes(this ITypeSymbol request)
        {
            INamedTypeSymbol result = request.GetResultType();

            if (result is null)
            {
                yield break;
            }

            var pending = new Queue<ITypeSymbol>();
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default) { request };
            pending.Enqueue(result);

            while (pending.Count > 0)
            {
                ITypeSymbol type = pending.Dequeue();

                if (!visited.Add(type))
                {
                    continue;
                }

                if (type is IArrayTypeSymbol array)
                {
                    pending.Enqueue(array.ElementType);
                }
                else if (type is INamedTypeSymbol named)
                {
                    foreach (ITypeSymbol argument in named.TypeArguments)
                    {
                        pending.Enqueue(argument);
                    }

                    if (!IsOutputReference(named, request))
                    {
                        continue;
                    }

                    yield return named;

                    foreach (IPropertySymbol property in named.GetProperties(IsBindableReferenceProperty))
                    {
                        pending.Enqueue(property.Type);
                    }
                }
            }
        }

        private static bool IsOutputReference(INamedTypeSymbol type, ITypeSymbol request)
        {
            return type.Arity == 0
                && type.SpecialType == SpecialType.None
                && (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct)
                && IsFeatureOutputType(type, request);
        }

        private static bool IsFeatureOutputType(INamedTypeSymbol type, ITypeSymbol request)
        {
            return SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, request.ContainingAssembly)
                || type.HasBinder();
        }
    }
}