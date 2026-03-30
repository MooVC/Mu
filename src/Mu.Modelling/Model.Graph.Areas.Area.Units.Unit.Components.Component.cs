namespace Mu.Modelling;

using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Muify.Domain;

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
                                public Qualifier Namespace => Components.Unit.Namespace;

                                public string ProjectName => Components.Unit.ProjectName;

                                public ImmutableArray<Directive> References => Value.Attributes
                                    .Select(attribute => attribute.Type)
                                    .Append(typeof(DescriptionAttribute))
                                    .Append(typeof(IdentityAttribute))
                                    .GetReferences(Namespace);
                            }
                        }
                    }
                }
            }
        }
    }
}