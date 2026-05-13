namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Ardalis.GuardClauses;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    public sealed partial class Attribute
        : IValidatableObject
    {
        public static readonly Attribute Undefined = new Attribute();

        internal Attribute()
        {
        }

        [Descriptor("DefaultedTo")]
        [Traverse(Scope = TraverseScope.None)]
        public Snippet Default { get; internal set; } = Snippet.Empty;

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        [Descriptor("OfType")]
        [Traverse(Scope = TraverseScope.None)]
        public Symbol Type { get; internal set; } = Symbol.Undefined;

        public static implicit operator Attribute((Name Name, Symbol Type) source)
        {
            Guard.Against.Conversion<(Name Name, Symbol Type), Attribute>(source);

            return new Attribute()
                .Named(source.Name)
                .OfType(source.Type);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            return validationContext
                .IncludeIf(!Default.IsEmpty, nameof(Default), _ => Default.IsSingleLine, Default)
                .And(nameof(Name), _ => !Name.IsUnnamed, Name)
                .And(nameof(Type), _ => !Type.IsUndefined, Type)
                .Results;
        }
    }
}