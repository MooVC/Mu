namespace Mu.Communications.PubSub;

using System;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Messaging;

public sealed class ReflectionPolicyDirectory
    : IPolicyDirectory
{
    private static readonly Type _enumerable = typeof(IEnumerable<>);
    private static readonly Type _policy = typeof(IPolicy<,>);
    private static readonly Type _wrapper = typeof(Policy<,>);

    public IPolicy? Find(Event @event, IServiceScope scope)
    {
        Type type = @event.GetType();
        Type[] arguments = type.GetGenericArguments();

        if (arguments.Length != 2)
        {
            throw new NotSupportedException($"Event type `{type.Name}` is not supported as it does not have the expected number of generic arguments.");
        }

        Type policy = _policy.MakeGenericType(arguments);
        Type enumerable = _enumerable.MakeGenericType(policy);

        object? instance = scope.ServiceProvider.GetService(enumerable);

        if (instance is not IEnumerable services)
        {
            return default;
        }

        Type wrapper = _wrapper.MakeGenericType(arguments);

        return (IPolicy?)Activator.CreateInstance(wrapper, services);
    }
}