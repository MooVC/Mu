namespace Mu.Modelling.Components.Feature;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Graphify;
using MooVC;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
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
        var content = Builder
            .New<Definition>()
            .For<Record>(record => Parameterize(feature, record)
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
                .Named(feature.Value.Name))
            .From(feature.Namespace)
            .ImportReferences(feature.Namespace)
            .ToSnippet(feature.Root.Options);

        yield return new File(content, Extensions.Code, feature.Value.Name, $"{Folders.Source}/{feature.ProjectName}/");
    }

    private static Record Parameterize(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, Record record)
    {
        if (feature.Value.Type.IsMutational && feature.Value.Mutational.Type.IsTransitional)
        {
            record = record.WithParameters(identity => identity
                .Named(nameof(feature.Features.Unit.Value.Identity))
                .OfType(feature.Features.Unit.Value.Identity));
        }

        return record.WithParameters(feature.Value.Parameters);
    }
}