namespace Mu.Modelling.Components.Domain;

using System.Runtime.CompilerServices;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Muify.Domain;
using Builder = MooVC.Syntax.Builder;

internal sealed class Aggregate
    : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
{
    public async IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var content = Builder
            .New<Definition>()
            .For<Record>(record => record
                .AttributedWith(aggregate => aggregate
                    .Named(name => name
                        .Named(typeof(UnitAttribute<>))
                        .WithArguments(identity => identity.Named(unit.Value.Identity))))
                .DescribedAs(unit.Value.Description)
                .Named(unit.Value.Name)
                .WithParameters(unit.Value.Attributes))
            .From(unit.Namespace)
            .ImportReferences(unit.Namespace)
            .ToSnippet(unit.Root.Options);

        yield return new File(content, Extensions.Code, unit.Value.Name, $"{Folders.Source}/{unit.ProjectName}/");
    }
}