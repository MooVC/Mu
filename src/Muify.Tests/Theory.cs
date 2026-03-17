namespace Muify;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

internal sealed record Theory(ReferenceAssemblies Assemblies, LanguageVersion Language);