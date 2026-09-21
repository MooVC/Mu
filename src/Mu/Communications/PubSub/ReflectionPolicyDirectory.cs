namespace Mu.Communications.PubSub;

using System;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using MooVC;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using static Mu.Communications.PubSub.ReflectionPolicyDirectory_Resources;

public sealed class ReflectionPolicyDirectory
    : IPolicyDirectory
{
    private static readonly Type _enumerable = typeof(IEnumerable<>);
    private static readonly Type _policy = typeof(IPolicy<,>);
    private static readonly Type _wrapper = typeof(Policy<,>);

    public IPolicy? Find(Event @event, IServiceProvider provider)
    {
        Type type = @event.GetType();
        Type[] arguments = type.GetGenericArguments();

        if (arguments.Length != 2)
        {
            throw new NotSupportedException(FindArgumentsCountRequired.Format(type.Name));
        }

        Type policy = _policy.MakeGenericType(arguments);
        Type enumerable = _enumerable.MakeGenericType(policy);

        object? instance = provider.GetService(enumerable);

        if (instance is not IEnumerable services)
        {
            return default;
        }

        Type wrapper = _wrapper.MakeGenericType(arguments);

        return (IPolicy?)Activator.CreateInstance(wrapper, services);
    }
}