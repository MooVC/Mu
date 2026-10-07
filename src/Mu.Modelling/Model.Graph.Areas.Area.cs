namespace Mu.Modelling
{
    using System.Collections.Immutable;
    using System.Linq;
    using MooVC.Syntax;
    using Mu.Modelling.Syntax.CSharp;

    public partial class Model
    {
        public static partial class Graph
        {
            public partial class Areas
            {
                public partial class Area
                {
                    public Qualifier Namespace => new Qualifier(ImmutableArray.Create(_Root.Company, _Root.Name, _Value.Name));

                    public string ProjectName => Namespace;

                    public ImmutableArray<Qualifier> Projects => _Value.Components
                        .SelectMany(component => component.Attributes)
                        .Select(attribute => attribute.Type)
                        .GetProjects(_Root.Company, _Root.Name, _Value.Name);
                }
            }
        }
    }
}