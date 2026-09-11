namespace Muify.Modelling
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using Mu.Modelling;

    internal static partial class ComponentExtensions
    {
        public static Component AttributedWith(this Component component, IEnumerable<IPropertySymbol> properties)
        {
            return component.Enumerate(AttributedWith, properties);
        }

        private static Component AttributedWith(IPropertySymbol property, Component component)
        {
            return component.AttributedWith(attribute => attribute
                .Named(property.Name)
                .OfType(property.Type));
        }
    }
}