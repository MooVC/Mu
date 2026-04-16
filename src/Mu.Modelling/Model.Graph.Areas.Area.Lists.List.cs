namespace Mu.Modelling
{
    using System.Diagnostics.CodeAnalysis;
    using MooVC.Syntax;

    public partial class Model
    {
        public static partial class Graph
        {
            public partial class Areas
            {
                public partial class Area
                {
                    [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "The class is auto-generated.")]
                    public partial class Lists
                    {
                        public sealed partial class List
                        {
                            public Qualifier Namespace => Lists.Area.Namespace;

                            public string ProjectName => Lists.Area.ProjectName;
                        }
                    }
                }
            }
        }
    }
}