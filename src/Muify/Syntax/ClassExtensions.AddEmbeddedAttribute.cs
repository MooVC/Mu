namespace Muify.Syntax
{
    using MooVC.Syntax.CSharp;

    internal static partial class ClassExtensions
    {
        public static Class AddEmbeddedAttribute(this Class @class)
        {
            return @class.AttributedWith(attribute => attribute
                .Named((Name: "EmbeddedAttribute", Qualifier: "Microsoft.CodeAnalysis")));
        }
    }
}