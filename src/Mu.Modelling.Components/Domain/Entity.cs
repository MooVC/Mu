namespace Mu.Modelling.Components.Domain;

using System.Collections.Immutable;
using MooVC.Modelling;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Attribute = Mu.Modelling.Attribute;
using Builder = MooVC.Syntax.Builder;
using Extensions = MooVC.Syntax.CSharp.Extensions;

internal sealed class Entity
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
        if (identifier.IsUndefined)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Class>(@class => @class
                .DescribedAs(description)
                .Named(name)
                .WithProperties(properties)
                .WithProperties(property => property
                    .AttributedWith(attribute => attribute.Named((Name: "IdentityAttribute", Qualifier: "Muify.Domain")))
                    .From(identifier)))
            .From(@namespace)
            .ImportReferences(@namespace)
            .ToSnippet(options);

        yield return new File(content, Extensions.Code, name, $"{Folders.Source}/{project}/");
    }
}