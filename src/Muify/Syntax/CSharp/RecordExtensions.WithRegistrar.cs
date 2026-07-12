namespace Muify.Syntax.CSharp
{
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;

    internal static partial class RecordExtensions
    {
        public static Record WithRegistrar(this Record record, Snippet body, Name name)
        {
            Symbol configuration = (Name: "IConfiguration", Qualifier: "Microsoft.Extensions.Configuration");
            Symbol container = (Name: "Container", Qualifier: "SimpleInjector");

            body = body.IsEmpty
                ? Snippet.From(Configuration.Options, "// There are no registrars defines within the assembly")
                : body;

            return record.Implements((Name: "IRegistrar", Qualifier: "Mu.Composition"))
                .Named(name)
                .WithMethods(register => register
                    .Accepts((Name: "Configuration", Type: configuration))
                    .Accepts((Name: "Container", Type: container))
                    .Named("Register")
                    .Returns(Result.Void)
                    .WithBody(body)
                    .WithExtensibility(Modifiers.Static));
        }
    }
}