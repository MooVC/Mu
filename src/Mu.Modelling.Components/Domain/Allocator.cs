namespace Mu.Modelling.Components.Domain;

using System.Runtime.CompilerServices;
using MooVC.Modelling;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Builder = MooVC.Syntax.Builder;
using Extensions = MooVC.Syntax.CSharp.Extensions;

internal sealed class Allocator
    : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
{
    public async IAsyncEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (unit.Value.Identity.IsType && unit.Value.Identity.Type == typeof(Guid))
        {
            yield break;
        }

        var content = Builder
            .New<Definition>()
            .For<Class>(@class => @class
                .Implements((Name: "IAllocator", Qualifier: "Mu.Modelling.Services"), @base => @base.WithArguments(unit.Value.Identity.GetSymbol(unit.Namespace)))
                .Named("Allocator")
                .WithMethods(allocate => DefineAllocate(allocate, unit))
                .WithMethods(confirm => DefineConfirm(confirm, unit))
                .WithMethods(surrender => DefineSurrender(surrender, unit)))
            .From(unit.Namespace)
            .ImportReferences(unit.Namespace)
            .ToSnippet(unit.Root.Options);

        yield return new File(content, Extensions.Code, "Allocator", $"{Folders.Source}/{unit.ProjectName}/");
    }

    private static Method DefineAllocate(Method allocate, Model.Graph.Areas.Area.Units.Unit unit)
    {
        return allocate
            .Accepts(useCase => useCase
                .Named("UseCase")
                .OfType((Name: "TUseCase", Qualifier: Qualifier.Unqualified)))
            .Accepts(cancellationToken => cancellationToken
                .Named("CancellationToken")
                .OfType(typeof(CancellationToken)))
            .Named("Allocate", declaration => declaration
                .WithArguments(useCase => useCase
                    .WithConstraints(constraint => constraint
                        .WithBase((Name: "UseCase", Qualifier: "Mu.Modelling.Behavior")))
                    .Named("TUseCase")))
            .Returns(result => result
                .OfType(typeof(ValueTask), task => task.WithArguments(unit.Value.Identity.GetSymbol(unit.Namespace)))
                .WithMode(Result.Modes.Synchronous))
            .WithExtensibility(Modifiers.Override)
            .WithBody("throw new NotImplementedException();")
            .WithScope(Scopes.Public);
    }

    private static Method DefineConfirm(Method confirm, Model.Graph.Areas.Area.Units.Unit unit)
    {
        return confirm
            .Accepts(identity => identity
                .Named("Identity")
                .OfType(unit.Value.Identity.GetSymbol(unit.Namespace)))
            .Accepts(cancellationToken => cancellationToken
                .Named("CancellationToken")
                .OfType(typeof(CancellationToken)))
            .Named("Confirm")
            .Returns(typeof(ValueTask), result => result.WithMode(Result.Modes.Synchronous))
            .WithExtensibility(Modifiers.Override)
            .WithBody("return ValueTask.CompletedTask;")
            .WithScope(Scopes.Public);
    }

    private static Method DefineSurrender(Method surrender, Model.Graph.Areas.Area.Units.Unit unit)
    {
        return surrender
            .Accepts(identity => identity
                .Named("Identity")
                .OfType(unit.Value.Identity.GetSymbol(unit.Namespace)))
            .Accepts(cancellationToken => cancellationToken
                .Named("CancellationToken")
                .OfType(typeof(CancellationToken)))
            .Named("Surrender")
            .Returns(typeof(ValueTask), result => result.WithMode(Result.Modes.Synchronous))
            .WithExtensibility(Modifiers.Override)
            .WithBody("return ValueTask.CompletedTask;")
            .WithScope(Scopes.Public);
    }
}