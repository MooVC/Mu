namespace Mu.Modelling.Components.Feature;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MooVC;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Builder = MooVC.Syntax.Builder;

internal sealed class Request
    : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
{
    public async IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, [EnumeratorCancellation] CancellationToken cancellationToken)
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
                            @true: type => type.Named((Name: "CreationalAttribute", Qualifier: "Muify.Service")),
                            @false: type => type.Named((Name: "TransitionalAttribute", Qualifier: "Muify.Service")))
                        .WithArguments((Name: nameof(feature.Value.Mutational.Fact), Value: $"\"{feature.Value.Mutational.Fact}\""))),
                    @false: record => record
                        .AttributedWith(attribute => attribute
                            .Named((Name: "NonMutationalAttribute", Qualifier: "Muify.Service"))))
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
                .OfType(feature.Features.Unit.Value.Identity.GetSymbol(feature.Namespace)));
        }

        return record.WithParameters(feature.Value.Parameters);
    }
}