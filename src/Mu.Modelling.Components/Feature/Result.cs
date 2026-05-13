namespace Mu.Modelling.Components.Feature;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MooVC;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Builder = MooVC.Syntax.Builder;
using ResultModel = Mu.Modelling.Result;

internal sealed class Result
    : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
{
    public async IAsyncEnumerable<File> Observe(
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ImmutableArray<ResultModel> results = feature.Value.Results;
        bool isCreational = feature.Value.Type.IsMutational && feature.Value.Mutational.Type.IsCreational;

        if (results.Length == 0 && !isCreational)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Record>(record => record
                .Containing(Type
                    .New<Record>()
                    .Named(nameof(Result))
                    .ForkOn(_ => isCreational, @true: result => CreateIdentity(feature, result), @false: _ => _)
                    .WithParameters(results))
                .Named(feature.Value.Name))
            .From(feature.Namespace)
            .ImportReferences(feature.Namespace)
            .ToSnippet(feature.Root.Options);

        yield return new File(content, Extensions.Code, $"{feature.Value.Name}.{nameof(Result)}", $"{Folders.Source}/{feature.ProjectName}/");
    }

    private static Record CreateIdentity(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, Record record)
    {
        string description = $"The {nameof(feature.Features.Unit.Value.Identity)} of the Newly Created {feature.Features.Unit.Value.Name}";

        return record.WithParameters(identity => identity
            .AttributedWith(
                typeof(DescriptionAttribute),
                attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\"")))
            .Named(nameof(feature.Features.Unit.Value.Identity))
            .OfType(feature.Features.Unit.Value.Identity));
    }
}