namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class CompilationExtensions
    {
        private static INamedTypeSymbol GetProjectDefinition(this Compilation compilation, string projectNamespace)
        {
            string[] segments = projectNamespace.Split('.');
            string name = segments[segments.Length - 1];

            return compilation.Assembly.GetTypeByMetadataName($"{projectNamespace}.{name}");
        }
    }
}