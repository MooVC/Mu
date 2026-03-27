namespace Mu.Modelling.Components.Domain;

using System.Collections.Immutable;
using Graphify;
using Monify;
using MooVC;
using MooVC.Modelling;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Builder = MooVC.Syntax.Builder;
using Extensions = MooVC.Syntax.CSharp.Extensions;

internal sealed class List
    : IVisitor<Model.Graph.Areas.Area.Lists.List, File>,
      IVisitor<Model.Graph.Areas.Area.Units.Unit.Lists.List, File>
{
    public IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Lists.List list, CancellationToken cancellationToken)
    {
        return Create(
            list.Value.Description,
            list.Value.Name,
            list.Namespace,
            list.Value.Members,
            list.ProjectName,
            list.Root.Options);
    }

    public IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Lists.List list, CancellationToken cancellationToken)
    {
        return Create(
            list.Value.Description,
            list.Value.Name,
            list.Namespace,
            list.Value.Members,
            list.ProjectName,
            list.Root.Options);
    }

    private static async IAsyncEnumerable<File> Create(
        Description description,
        Name name,
        Qualifier @namespace,
        ImmutableArray<Name> members,
        string project,
        Options options)
    {
        if (members.IsDefaultOrEmpty)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .From(@namespace)
            .For<Record>(record => record
                .AttributedWith(monify => monify.Named(attribute => attribute
                    .From(typeof(MonifyAttribute))
                    .Named(nameof(MonifyAttribute))
                    .WithArguments(type => type.Named(typeof(byte)))))
                .DescribedAs(description)
                .Named(name)
                .Enumerate(
                    member => record
                        .WithFields(field => field
                            .IsReadOnly(true)
                            .IsStatic(true)
                            .Named(member)
                            .OfType((Name: member, Qualifier: @namespace)))
                        .WithProperties(property => property
                            .WithBehaviours(methods => methods
                                .WithGet($"this == {member}")
                                .WithSet(setter => setter.WithMode(Property.Mode.ReadOnly)))
                            .Named($"Is{name}")
                            .OfType(typeof(bool))),
                    members)
                .WithConstructors(constructor => constructor
                    .WithBody("_value = value")
                    .WithParameters((Name: "Value", Type: typeof(byte)))
                    .WithScope(Scope.Private)))
            .Referencing(directive => directive.From(typeof(MonifyAttribute)))
            .ToSnippet(options);

        yield return new File(content, Extensions.Code, name, $"{Folders.Source}/{project}/");
    }
}