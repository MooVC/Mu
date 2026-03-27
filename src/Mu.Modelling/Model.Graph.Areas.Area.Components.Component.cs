namespace Mu.Modelling;

using System.Collections.Immutable;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;

public partial class Model
{
    public static partial class Graph
    {
        public partial class Areas
        {
            public partial class Area
            {
                public partial class Components
                {
                    public partial class Component
                    {
                        public Qualifier Namespace => Components.Area.Namespace;

                        public string ProjectName => Components.Area.ProjectName;

                        public ImmutableArray<Directive> References => Value.Attributes
                            .Select(attribute => attribute.Type)
                            .GetReferences(Namespace);
                    }
                }
            }
        }
    }
}