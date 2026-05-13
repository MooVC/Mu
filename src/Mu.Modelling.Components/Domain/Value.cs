namespace Mu.Modelling.Components.Domain;

using System.Collections.Immutable;
using MooVC.Modelling;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Attribute = Mu.Modelling.Attribute;
using Builder = MooVC.Syntax.Builder;
using Extensions = MooVC.Syntax.CSharp.Extensions;

internal sealed class Value
    : IModelVisitor<Model.Graph.Areas.Area.Components.Component, File>,
      IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Components.Component, File>
{
    public IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Components.Component component, CancellationToken cancellationToken)
    {
        return Create(
            component.Value.Description,
            component.Value.Identifier,
            component.Value.Name,
            component.Namespace,
            component.ProjectName,
            component.Value.Attributes,
            component.Root.Options);
    }

    public IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Components.Component component, CancellationToken cancellationToken)
    {
        return Create(
            component.Value.Description,
            component.Value.Identifier,
            component.Value.Name,
            component.Namespace,
            component.ProjectName,
            component.Value.Attributes,
            component.Root.Options);
    }

    private static async IAsyncEnumerable<File> Create(
        Description description,
        Attribute identifier,
        Name name,
        Qualifier @namespace,
        string project,
        ImmutableArray<Attribute> properties,
        Options options)
    {
        if (!identifier.IsUndefined)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Record>(record => record
                .DescribedAs(description)
                .Named(name)
                .WithParameters(properties))
            .From(@namespace)
            .ImportReferences(@namespace)
            .ToSnippet(options);

        yield return new File(content, Extensions.Code, name, $"{Folders.Source}/{project}/");
    }
}