namespace Mu.Composition;

using System;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using static Mu.Composition.WebApplicationBuilderExtensions_Resources;

public static partial class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder ConfigureMu(Action<KestrelServerOptions>? kestrel = default)
        {
            _ = Guard.Against.Null(builder, message: ConfigureMuBuilderRequired);

            kestrel ??= options => options.ListenAnyIP(50051, listen => listen.Protocols = HttpProtocols.Http2);

            _ = builder.WebHost.ConfigureKestrel(kestrel);

            return builder;
        }
    }
}