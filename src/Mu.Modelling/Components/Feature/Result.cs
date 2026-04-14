namespace Mu.Modelling.Components.Feature;

extern alias Framework;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Graphify;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Builder = MooVC.Syntax.Builder;
using ResultModel = Mu.Modelling.Result;

internal sealed class Result
    : IVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
{
    public async IAsyncEnumerable<File> Observe(
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ImmutableArray<ResultModel> results = feature.Value.Results;

        if (feature.Value.Type.IsMutational && feature.Value.Mutational.Type.IsCreational)
        {
            var identity = new ResultModel
            {
                Description = $"The {nameof(feature.Features.Unit.Value.Identity)} of the Newly Created {feature.Features.Unit.Value.Name}",
                Name = nameof(feature.Features.Unit.Value.Identity),
                Type = feature.Features.Unit.Value.Identity,
            };

            results = [.. results, identity];
        }

        if (results.Length == 0)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Record>(record => record
                .Containing(Type
                    .New<Record>()
                    .Named(nameof(Result))
                    .WithParameters(results))
                .Named(feature.Value.Name))
            .From(feature.Namespace)
            .ImportReferences(feature.Namespace)
            .ToSnippet(feature.Root.Options);

        yield return new File(content, Extensions.Code, $"{feature.Value.Name}.{nameof(Result)}", $"{Folders.Source}/{feature.ProjectName}/");
    }
}