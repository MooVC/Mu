namespace Mu.Modelling.Components.Domain;

using System.Runtime.CompilerServices;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Builder = MooVC.Syntax.Builder;
using Extensions = MooVC.Syntax.CSharp.Extensions;

internal sealed class Identity
    : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Identity, File>
{
    public async IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Identity identity, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!identity._Value.IsComponent)
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Struct>(@struct => @struct
                .DescribedAs(identity._Value.Component.Description)
                .Named(identity._Value.Component.Name)
                .WithBehavior(Struct.Kinds.ReadOnly + Struct.Kinds.Record)
                .WithProperties(identity._Value.Component.Attributes))
            .From(identity.Unit.Namespace)
            .ImportReferences(identity.Unit.Namespace)
            .ToSnippet(identity.Unit._Root.Options);

        yield return new File(content, Extensions.Code, identity._Value.Component.Name, $"{Folders.Source}/{identity.Unit.ProjectName}/");
    }
}