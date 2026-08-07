namespace Mu.Modelling.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Ardalis.GuardClauses;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

public sealed class ReflectionTransform<TAggregate>(IServiceProvider provider)
    : ITransform<TAggregate>
    where TAggregate : Aggregate
{
    private static readonly Type _aggregate = typeof(TAggregate);
    private static readonly Type _enumerable = typeof(IEnumerable<>);
    private static readonly Type _transform = typeof(ITransform<,>);
    private static readonly Type _wrapper = typeof(Transform<,>);

    public TAggregate Apply(TAggregate aggregate, Fact fact)
    {
        _ = Guard.Against.Null(aggregate, message: $"The `{typeof(TAggregate)}` to which the fact is being applied must be provided.");
        _ = Guard.Against.Null(fact, message: $"The fact to be applied to `{typeof(TAggregate)}` must be provided.");

        Type type = fact.GetType();
        Type transform = _transform.MakeGenericType(_aggregate, type);
        object? matches = provider.GetService(_enumerable.MakeGenericType(transform));

        if (matches is not IEnumerable transforms)
        {
            return aggregate;
        }

        Type wrapper = _wrapper.MakeGenericType(_aggregate, type);

        ITransform<TAggregate> instance = (ITransform<TAggregate>?)Activator.CreateInstance(wrapper, [transforms])
            ?? throw new InvalidOperationException($"The transform for `{typeof(TAggregate)}` and `{type}` could not be created.");

        return instance.Apply(aggregate, fact);
    }
}