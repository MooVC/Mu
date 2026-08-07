namespace Mu.Composition;

using System;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureMu(this WebApplicationBuilder builder, Action<KestrelServerOptions>? kestrel = default)
    {
        _ = Guard.Against.Null(builder, message: "The builder to configure must be provided.");

        kestrel ??= options => options.ListenAnyIP(50051, listen => listen.Protocols = HttpProtocols.Http2);

        _ = builder.WebHost.ConfigureKestrel(kestrel);

        return builder;
    }
}