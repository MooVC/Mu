namespace Mu.Composition;

using System;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;
using static Mu.Composition.WebApplicationBuilderExtensions_Resources;

public static partial class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplication BuildMu(
            Action<SimpleInjectorAddOptions>? add = default,
            Action<KestrelServerOptions>? kestrel = default,
            Action<SimpleInjectorUseOptions>? use = default)
        {
            return builder.BuildMu(out _, add, kestrel, use);
        }

        public WebApplication BuildMu(
            out Container container,
            Action<SimpleInjectorAddOptions>? add = default,
            Action<KestrelServerOptions>? kestrel = default,
            Action<SimpleInjectorUseOptions>? use = default)
        {
            _ = Guard.Against.Null(builder, message: BuildMuBuilderRequired);

            var composition = CompositionContext.Find(builder.Services);

            if (composition is null)
            {
                _ = builder.Services.AddMu(out container, options: add);
            }
            else
            {
                container = composition.Container;

                if (add is not null && composition.IntegrationOptions is not null)
                {
                    add.Invoke(composition.IntegrationOptions);
                }
            }

            WebApplication host = builder
                .ConfigureMu(kestrel: kestrel)
                .Build();

            _ = host.UseMu(container, options: use, verify: false);

            return host;
        }
    }
}