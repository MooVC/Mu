namespace Mu.Modelling
{
    using System;
    using Ardalis.GuardClauses;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using MooVC.Syntax.Validation;
    using Valuify;

    [Valuify]
    public sealed partial class Identity
    {
        public static readonly Identity Default = new Identity();

        internal Identity()
        {
        }

        public Component Component { get; internal set; } = Component.Undefined;

        [Ignore]
        public bool IsComponent => Component != Component.Undefined;

        [Ignore]
        public bool IsDefault => this == Default;

        [Ignore]
        public bool IsType => Component == Component.Undefined;

        public Qualification Type { get; internal set; } = typeof(Guid);

        public static implicit operator Identity(Component component)
        {
            Guard.Against.Conversion<Component, Identity>(component);

            return new Identity()
            {
                Component = component,
            };
        }

        public static implicit operator Component(Identity identity)
        {
            Guard.Against.Conversion<Identity, Component>(identity);

            return identity.Component;
        }

        public static implicit operator Identity(Qualification type)
        {
            Guard.Against.Conversion<Qualification, Identity>(type);

            return new Identity()
            {
                Type = type,
            };
        }

        public static implicit operator Qualification(Identity identity)
        {
            Guard.Against.Conversion<Identity, Qualification>(identity);

            return identity.Type;
        }

        public Symbol GetSymbol(Qualifier @namespace)
        {
            _ = Guard.Against.Null(@namespace, message: "The namespace must be provided.");

            if (IsComponent)
            {
                return (Component.Name, Qualifier: @namespace);
            }

            return Type;
        }
    }
}