namespace Muify.Syntax.CSharp
{
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using static Muify.Configuration;

    internal static partial class SymbolExtensions
    {
        public static string ToBinding(this Symbol symbol, bool whenUndefined = false)
        {
            Symbol model = (Name: "RuntimeTypeModel.Default", Qualifier: "ProtoBuf.Meta");
            string binding = $"_ = {Render(symbol)}.Bind({Render(model)});";

            if (!whenUndefined)
            {
                return binding;
            }

            Symbol extensions = (Name: "RuntimeTypeModelExtensions", Qualifier: "Mu.Serialization");

            return Snippet.From(Configuration.Options, binding)
                .Block(Configuration.Options, $"if (!{Render(extensions)}.HasBinding<{Render(symbol)}>({Render(model)}))");
        }
    }
}