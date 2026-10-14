namespace Mu.Modelling
{
    using System.Collections.Immutable;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax.CSharp;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    public partial class Model
    {
        [Fluentify]
        [Valuify]
        internal sealed partial class Semantics
        {
            public static readonly Semantics OutOfScope = new Semantics();

            [Traverse(Scope = TraverseScope.None)]
            public string ApplicationNamespace { get; set; } = string.Empty;

            [Traverse(Scope = TraverseScope.None)]
            public ImmutableArray<string> Assemblies { get; set; } = ImmutableArray<string>.Empty;

            [Traverse(Scope = TraverseScope.None)]
            public ImmutableArray<Qualification> Binders { get; set; } = ImmutableArray<Qualification>.Empty;

            [Traverse(Scope = TraverseScope.None)]
            public string BuildName { get; set; } = string.Empty;

            [Traverse(Scope = TraverseScope.None)]
            public ImmutableArray<Qualification> Features { get; set; } = ImmutableArray<Qualification>.Empty;

            [Traverse(Scope = TraverseScope.None)]
            public bool HasApplicationBuildExtension { get; set; }

            [Traverse(Scope = TraverseScope.None)]
            public bool HasBinder { get; set; }

            [Traverse(Scope = TraverseScope.None)]
            public bool HasBuilderBuildExtension { get; set; }

            [Traverse(Scope = TraverseScope.None)]
            public bool HasProgram { get; set; }

            [Traverse(Scope = TraverseScope.None)]
            public bool IsExecutable { get; set; }

            [Traverse(Scope = TraverseScope.None)]
            public bool IsHost { get; set; }

            [Ignore]
            public bool IsOutOfScope => this == OutOfScope;

            [Traverse(Scope = TraverseScope.None)]
            public bool IsSharedProject { get; set; }

            [Traverse(Scope = TraverseScope.None)]
            public ImmutableArray<Poco> SharedTypes { get; set; } = ImmutableArray<Poco>.Empty;
        }
    }
}