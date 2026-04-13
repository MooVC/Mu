namespace Mu.Modelling.Components.Feature;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Graphify;
using MooVC;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Muify.Service;
using Builder = MooVC.Syntax.Builder;
using Parameter = Mu.Modelling.Parameter;

internal sealed class Request
    : IVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
{
    public async IAsyncEnumerable<File> Observe(
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ImmutableArray<Parameter> parameters = GetParameters(feature);

        var content = Builder
            .New<Definition>()
            .For<Record>(record => record
                .DescribedAs(feature.Value.Description)
                .ForkOn(
                    _ => feature.Value.Type.IsMutational,
                    @true: record => record.AttributedWith(type => type
                        .ForkOn(
                            _ => feature.Value.Mutational.Type.IsCreational,
                            @true: type => type.Named(typeof(CreationalAttribute)),
                            @false: type => type.Named(typeof(TransitionalAttribute)))
                        .WithArguments((Name: nameof(feature.Value.Mutational.Fact), Value: $"\"{feature.Value.Mutational.Fact}\""))),
                    @false: record => record.AttributedWith(typeof(NonMutationalAttribute)))
                .Named(feature.Value.Name)
                .WithParameters(parameters))
            .From(feature.Namespace)
            .ImportReferences(feature.Namespace)
            .ToSnippet(feature.Root.Options);

        yield return new File(content, Extensions.Code, feature.Value.Name, $"{Folders.Source}/{feature.ProjectName}/");
    }

    private static ImmutableArray<Parameter> GetParameters(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
    {
        ImmutableArray<Parameter> parameters = feature.Value.Parameters;

        if (feature.Value.Type.IsMutational && feature.Value.Mutational.Type.IsTransitional)
        {
            var identity = new Parameter
            {
                Name = nameof(feature.Features.Unit.Value.Identity),
                Type = feature.Features.Unit.Value.Identity,
            };

            parameters = [.. parameters, identity];
        }

        return parameters;
    }
}