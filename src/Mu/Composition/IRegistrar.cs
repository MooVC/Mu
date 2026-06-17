namespace Mu.Composition;

using Microsoft.Extensions.Configuration;
using SimpleInjector;

public interface IRegistrar
{
    static abstract Container Register(IConfiguration configuration, Container container);
}