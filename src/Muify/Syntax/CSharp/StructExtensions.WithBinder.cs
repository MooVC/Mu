namespace Muify.Syntax.CSharp
{
    using System.ComponentModel;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Characteristics = Mu.Modelling.Characteristics;

    internal static partial class StructExtensions
    {
        public static Struct WithBinder(this Struct @struct, Snippet body, Characteristics characteristics, Name name)
        {
            Symbol model = (Name: "RuntimeTypeModel", Qualifier: "ProtoBuf.Meta");

            Struct.Kinds behaviors = Struct.Kinds.Default;

            if (characteristics.IsReadOnly)
            {
                behaviors += Struct.Kinds.ReadOnly;
            }

            if (characteristics.IsRecord)
            {
                behaviors += Struct.Kinds.Record;
            }

            if (characteristics.IsRef)
            {
                behaviors += Struct.Kinds.Ref;
            }

            body = body.IsEmpty
                ? Snippet.From(Configuration.Options, "// There are no properties to map")
                : body;

            return @struct.Implements((Name: "IBinder", Qualifier: "Mu.Serialization"))
                .Named(name)
                .WithMethods(register => register
                    .Accepts((Name: "Model", Type: model))
                    .Named("Bind")
                    .Returns(result => result
                        .OfType(model)
                        .WithMode(Result.Modes.Synchronous))
                    .WithBody(body)
                    .WithExtensibility(Modifiers.Static))
                .WithBehavior(behaviors)
                .WithScope(Scopes.Unspecified);
        }
    }
}