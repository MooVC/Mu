namespace Mu.Composition;

using SimpleInjector;

internal static class RegistrationFallbacks
{
    private static readonly object _registrations = new();

    public static Container Register<TService, TImplementation>(Container container, Lifestyle lifestyle)
        where TService : class
        where TImplementation : class, TService
    {
        HashSet<Type> registrations = container.ContainerScope.GetOrSetItem(_registrations, static (_, _) => new HashSet<Type>());

        if (!registrations.Contains(typeof(TService)))
        {
            var registration = new Lazy<Registration>(() => lifestyle.CreateRegistration<TImplementation>(container));

            container.ResolveUnregisteredType += (_, arguments) =>
            {
                if (!arguments.Handled && arguments.UnregisteredServiceType == typeof(TService))
                {
                    arguments.Register(registration.Value);
                }
            };

            _ = registrations.Add(typeof(TService));
        }

        return container;
    }
}