namespace Mu.Modelling;

using System.Collections.Immutable;
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
                        public bool HasKernel => Units.Area.Value.Components.Length > 0;

                        public string KernelName => Units.Area.Namespace;

                        public Qualifier Namespace => Units.Area.Namespace.Append(Value.Name);

                        public string ProjectName => Namespace;

                        public ImmutableArray<Qualifier> Projects => Value.Attributes
                            .Select(attribute => attribute.Type)
                            .Union(Value.Components
                                .SelectMany(component => component.Attributes)
                                .Select(attribute => attribute.Type))
                            .Union(Value.Features
                                .SelectMany(feature => feature.Parameters)
                                .Select(parameter => parameter.Type))
                            .Union(Value.Features
                                .SelectMany(feature => feature.Results)
                                .Select(result => result.Type))
                             .Union(Value.Views
                                .SelectMany(view => view.Attributes)
                                .Select(view => view.Type))
                            .GetProjects(Root.Company, Root.Name, Units.Area.Value.Name, Value.Name);
                    }
                }
            }
        }
    }
}