namespace Mu.Modelling;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MooVC.Linq;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using MooVC.Syntax.Formatting;

public partial class Model
{
    public static partial class Graph
    {
        public partial class Areas
        {
            public partial class Area
            {
                public partial class Units
                {
                    public partial class Unit
                    {
                        [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "Class is auto-generated.")]
                        public partial class Components
                        {
                            public partial class Component
                            {
                                public Qualifier Namespace => new([Root.Company, Root.Name, Area.Name, Value.Name]);

                                public string ProjectName => Separator.Combine(Root.Company, Root.Name, Area.Name, Value.Name);

                                public ImmutableArray<Directive> References => Value.Attributes
                                    .Select(attribute => attribute.Type)
                                    .GetReferences(Namespace);
                            }
                        }
                    }
                }
            }
        }
    }
}