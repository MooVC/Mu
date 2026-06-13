namespace Mu.Modelling.Syntax.CSharp
{
    using System;
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.PropertyExtensions_Resources;
    using Attribute = Mu.Modelling.Attribute;
    using Parameter = Mu.Modelling.Parameter;

    public static partial class PropertyExtensions
    {
        public static Property From(this Property property, Attribute attribute)
        {
            _ = Guard.Against.Null(property, message: FromPropertyRequired);
            _ = Guard.Against.Null(attribute, message: FromAttributeRequired);

            return property
                .DescribedAs(attribute.Description)
                .Named(attribute.Name)
                .OfType(attribute.Type.AsPreferred())
                .WithDefault(attribute.Default);
        }

        public static Property From(this Property property, Parameter parameter)
        {
            _ = Guard.Against.Null(property, message: FromPropertyRequired);
            _ = Guard.Against.Null(parameter, message: FromParameterRequired);

            return property
                .DescribedAs(parameter.Description)
                .Named(parameter.Name.ToString())
                .OfType(parameter.Type.AsPreferred())
                .WithDefault(parameter.Default);
        }
    }
}