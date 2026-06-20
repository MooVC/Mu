namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Result = MooVC.Syntax.CSharp.Result;

    internal sealed class UnitRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit)
        {
            if (unit.Value.Metadata.IsOutOfScope || unit.Value.Metadata.HasRegistrar)
            {
                yield break;
            }

            Symbol configuration = (Name: "IConfiguration", Qualifier: "Microsoft.Extensions.Configuration");
            Symbol container = (Name: "Container", Qualifier: "SimpleInjector");

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .Implements((Name: "IRegistrar", Qualifier: "Mu.Composition"))
                    .Named(unit.Value.Name)
                    .WithMethods(register => register
                        .Accepts((Name: "Configuration", Type: configuration))
                        .Accepts((Name: "Container", Type: container))
                        .Named("Register")
                        .Returns(result => result
                            .OfType(container)
                            .WithMode(Result.Modes.Synchronous))
                        .WithExtensibility(Modifiers.Static)
                        .WithBody("return container;")))
                .From(unit.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Registrar");
        }
    }
}