namespace Mu.Modelling.Components.Feature;

using System.Runtime.CompilerServices;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using MooVC.Syntax.Project;
using Mu.Modelling.Components.Syntax.Project;
using Builder = MooVC.Syntax.Builder;
using Template = MooVC.Syntax.Project.Project;

internal sealed class Project
    : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
{
    public async IAsyncEnumerable<File> Observe(
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string content = Builder
            .New<Template>()
            .DescribedAs(feature.Value.Description)
            .WithItemGroups(group => group
                .WithProject($"{Folders.Source}/{feature.DomainName}/{feature.DomainName}.{Extensions.Project}"))
            .ToString();

        yield return new File(content, Extensions.Project, feature.ProjectName, $"{Folders.Source}/{feature.ProjectName}/");
    }
}