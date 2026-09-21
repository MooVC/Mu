namespace Mu.Modelling.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using MooVC;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using static Mu.Modelling.Services.ReflectionTransform_Resources;

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
        _ = Guard.Against.Null(aggregate, message: ApplyAggregateRequired.Format(typeof(TAggregate)));
        _ = Guard.Against.Null(fact, message: ApplyFactRequired.Format(typeof(TAggregate)));

        Type type = fact.GetType();
        Type transform = _transform.MakeGenericType(_aggregate, type);
        object? matches = provider.GetService(_enumerable.MakeGenericType(transform));

        if (matches is not IEnumerable transforms)
        {
            return aggregate;
        }

        Type wrapper = _wrapper.MakeGenericType(_aggregate, type);

        ITransform<TAggregate> instance = (ITransform<TAggregate>?)Activator.CreateInstance(wrapper, [transforms])
            ?? throw new InvalidOperationException(ApplyInstanceRequired.Format(typeof(TAggregate), type));

        return instance.Apply(aggregate, fact);
    }
}