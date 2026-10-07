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
                    public partial class Units
                    {
                        public sealed partial class Unit
                        {
                            public bool HasKernel => Units.Area._Value.Components.Length > 0;

                            public string KernelName => Units.Area.Namespace;

                            public Qualifier Namespace => Units.Area.Namespace.Append(_Value.Name);

                            public string ProjectName => Namespace;

                            public ImmutableArray<Qualifier> Projects => _Value.Attributes
                                .Select(attribute => attribute.Type)
                                .Union(_Value.Components
                                    .SelectMany(component => component.Attributes)
                                    .Select(attribute => attribute.Type))
                                .Union(_Value.Features
                                    .SelectMany(feature => feature.Parameters)
                                    .Select(parameter => parameter.Type))
                                .Union(_Value.Features
                                    .SelectMany(feature => feature.Results)
                                    .Select(result => result.Type))
                                 .Union(_Value.Views
                                    .SelectMany(view => view.Attributes)
                                    .Select(view => view.Type))
                                .GetProjects(_Root.Company, _Root.Name, Units.Area._Value.Name, _Value.Name);
                        }
                    }
                }
            }
        }
    }
}