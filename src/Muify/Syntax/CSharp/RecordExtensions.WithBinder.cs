namespace Muify.Syntax.CSharp
{
    using System.ComponentModel;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;

    internal static partial class RecordExtensions
    {
        public static Record WithBinder(this Record record, Snippet body, Name name)
        {
            Symbol model = (Name: "RuntimeTypeModel", Qualifier: "ProtoBuf.Meta");

            body = body.IsEmpty
                ? Snippet.From(Configuration.Options, "// There are no properties to map")
                : body;

            return record.Implements((Name: "IBinder", Qualifier: "Mu.Serialization"))
                .Named(name)
                .WithMethods(register => register
                    .Accepts((Name: "Model", Type: model))
                    .Named("Bind")
                    .Returns(result => result
                        .OfType(model)
                        .WithMode(Result.Modes.Synchronous))
                    .WithBody(body)
                    .WithExtensibility(Modifiers.Static));
        }
    }
}