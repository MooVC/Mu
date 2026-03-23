namespace Mu.Modelling.Components.Domain;

extern alias Framework;

using System.Runtime.CompilerServices;
using Graphify;
using MooVC.Modelling;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Base = Framework::Mu.Modelling.State.Aggregate;
using Builder = MooVC.Syntax.Builder;

internal sealed class Aggregate
    : IVisitor<Model.Graph.Areas.Area.Units.Unit, File>
{
    public async IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var content = Builder
            .New<Definition>()
            .From(unit.Namespace)
            .For<Record>(record => record
                .DescribedAs(unit.Value.Description)
                .DerivesFrom(typeof(Base))
                .Named(unit.Value.Name)
                .WithParameters(unit.Value.Attributes))
            .Referencing([.. unit.References])
            .ToSnippet(unit.Root.Options);

        yield return new File(content, Extensions.Code, unit.Value.Name, $"{Folders.Source}/{unit.ProjectName}/");
    }
}