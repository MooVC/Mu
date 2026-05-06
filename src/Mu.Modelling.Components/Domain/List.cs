namespace Mu.Modelling.Components.Domain;

using System.Collections.Immutable;
using Graphify;
using Monify;
using MooVC;
using MooVC.Modelling;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
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
        ImmutableArray<Member> members,
        string project,
        Options options)
    {
        if (members.IsDefaultOrEmpty)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Struct>(@struct => @struct
                .AttributedWith(monify => monify.Named(
                    typeof(MonifyAttribute),
                    monify => monify.WithArguments(typeof(string))))
                .DescribedAs(description)
                .Enumerate(
                    (member, @struct) => @struct
                        .WithFields(field => field
                            .DescribedAs(member.Description)
                            .IsReadOnly(true)
                            .IsStatic(true)
                            .Named(member.Name)
                            .OfType((Name: name, Qualifier: @namespace))
                            .WithDefault($"\"{{nameof({member.Name})}}\"")
                            .WithScope(Scopes.Public))
                        .WithProperties(property => property
                            .WithBehaviours(methods => methods
                                .WithGet($"this == {member.Name};")
                                .WithSet(setter => setter.WithMode(Property.Methods.Setter.Modes.ReadOnly)))
                            .Named($"Is{member.Name}")
                            .OfType(typeof(bool))),
                    members)
                .Named(name)
                .WithBehavior(Struct.Kinds.ReadOnly + Struct.Kinds.Record)
                .WithConstructors(constructor => constructor
                    .WithBody("_value = value;")
                    .WithParameters((Name: "Value", Type: typeof(string)))
                    .WithScope(Scopes.Private)))
            .From(@namespace)
            .ImportReferences(@namespace)
            .ToSnippet(options);

        yield return new File(content, Extensions.Code, name, $"{Folders.Source}/{project}/");
    }
}