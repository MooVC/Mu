namespace Mu.Modelling;

using MooVC.Syntax;

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
                    }
                }
            }
        }
    }
}