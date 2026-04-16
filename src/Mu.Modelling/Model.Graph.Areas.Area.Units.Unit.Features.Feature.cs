namespace Mu.Modelling
{
    using MooVC.Syntax;

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
                            public partial class Features
                            {
                                public partial class Feature
                                {
                                    public string DomainName => Features.Unit.ProjectName;

                                    public Qualifier Namespace => Features.Unit.Namespace.Append(Value.Name);

                                    public string ProjectName => Namespace;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}