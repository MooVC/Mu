namespace Muify.Syntax.CSharp
{
    using MooVC.Syntax.CSharp;
    using static Muify.Configuration;

    internal static partial class SymbolExtensions
    {
        public static string ToBinding(this Symbol symbol)
        {
            Symbol model = (Name: "RuntimeTypeModel.Default", Qualifier: "ProtoBuf.Meta");

            return $"_ = {Render(symbol)}.Bind({Render(model)});";
        }
    }
}