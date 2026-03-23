namespace Mu.Modelling;

extern alias Framework;

using System.Collections.Immutable;
using System.ComponentModel;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using MooVC.Syntax.Formatting;
using Aggregate = Framework::Mu.Modelling.State.Aggregate;

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
                        public string KernelName => Separator.Combine(Root.Company, Root.Name, Area.Name);

                        public Qualifier Namespace => new([Root.Company, Root.Name, Area.Name, Value.Name]);

                        public string ProjectName => Separator.Combine(Root.Company, Root.Name, Area.Name, Value.Name);

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
                            .GetProjects(Root.Company, Root.Name, Area.Name, Value.Name);

                        public ImmutableArray<Directive> References => Value.Attributes
                            .Select(attribute => attribute.Type)
                            .Append(typeof(Aggregate))
                            .Append(typeof(DescriptionAttribute))
                            .GetReferences(Namespace);
                    }
                }
            }
        }
    }
}