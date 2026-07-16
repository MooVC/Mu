namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using static Muify.Configuration;

    internal sealed class UnitIdentityRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Identity, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Identity identity)
        {
            if (identity.Unit.Value.Metadata.Allocator.IsUndefined || identity.Unit.Value.Metadata.Allocator.HasRegistrar)
            {
                yield break;
            }

            Service allocator = identity.Unit.Value.Metadata.Allocator;

            Symbol contract = Symbol.Undefined
                .Named((Name: "IAllocator", Qualifier: "Mu.Modelling.Services"))
                .WithArguments(identity.Value.GetSymbol(identity.Unit.Namespace));

            string predicate = $"context => context.Consumer.ImplementationType.Namespace.StartsWith(\"{identity.Unit.Namespace}\", StringComparison.Ordinal)";
            string registration = $"container.RegisterConditional<{Render(contract)}, {Render(allocator.Definition)}>({Render(LifeStyles.Scoped)}, {predicate});";
            string body = Snippet.From(Configuration.Options, registration);

            string content = Builder
                .New<Definition>()
                .For<Class>(@class => @class.WithRegistrar(body, allocator.Definition.Moniker))
                .From(identity.Unit.Namespace)
                .Referencing((Alias: string.Empty, Qualifier: "System"))
                .Referencing((Alias: string.Empty, Qualifier: "SimpleInjector"))
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Allocator.Registrar");
        }
    }
}