# Executable features and hosts

A Mu application consists of a domain project containing its aggregate and a feature project containing its request and optional result. Muify generates the protobuf binders, registrars, services, configuration, and application composition.

The existing feature naming convention is `Company.Project.Area.Unit.Feature`: the domain assembly is `Company.Project.Area.Unit`, and the request is `Feature` in the feature assembly's namespace. Muify's MSBuild props default conventional feature projects and projects ending in `.Host` to `OutputType=Exe`. An explicit project `OutputType` takes precedence; library projects keep their composition extensions without a generated entry point.

If a differently named project sets its host `RootNamespace` or conventional feature `AssemblyName` only inside its project file, also set `<OutputType>Exe</OutputType>` there. Executable defaults are selected during the early SDK imports; names supplied earlier in `Directory.Build.props` are detected automatically.

## Standalone feature

For `MooVC.Testing.Modelling.Account.Open`, Muify generates `BuildOpen()` on `WebApplicationBuilder` and `WebApplication`, together with a `Program.g.cs` entry point. The generated program creates a builder using the command-line arguments, calls `BuildOpen()`, and runs the application. No handwritten program is required.

A custom program can use the same extension:

```csharp
using Microsoft.AspNetCore.Builder;
using MooVC.Testing.Modelling.Account.Open;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.BuildOpen();
await app.RunAsync();
```

Hosted features run their domain registrar before their feature registrar. Missing persistence services use an in-memory stream and reflection-based aggregate replay; missing shared services use the in-memory auditor and mediator. Feature options determine whether the application hosts a mapped gRPC service or consumes a remote feature. Configuration sections, including gRPC ports, limits, addresses, and timeouts, are read from the builder's configuration.

Request protobuf binders preserve causal identity and proposal time, reserve field 3 for the formerly serialized computed model, and keep payload fields starting at 4. Transitional targets are appended after the payload fields. The model remains derived from the request's aggregate type, and deserialization restores metadata through non-public initialization accessors.

## Host project

A host's final namespace node is `Host`, with its build extension named after the preceding node. Both `My.Project.Host` and `MooVC.Testing.Modelling.Account.Host` are supported, generating `BuildProject()` and `BuildAccount()` respectively. The configured `RootNamespace` is used when provided; otherwise the assembly name is used.

Reference the feature projects from the host and include Mu and the Muify analyzer. Muify discovers referenced feature requests with callable application build methods, sorts them by qualification, and generates a host entry point and composition extensions. Ordinary referenced libraries are excluded. Each hosted feature's unit registrar runs once per container, including when several features reference the same domain.

For the account host, a custom program's composition is simply `builder.BuildAccount()`. The application extensions also support manual chaining such as `builder.BuildMu().BuildOpen().BuildClose()` before startup.

## Overrides

An included `Program.cs`, top-level statements, or an existing entry point suppresses the generated program. A compatible public build method suppresses only that receiver's generated extension; application and builder methods can be replaced independently. Classic extension methods and C# 14 extension blocks are supported. Build methods return `WebApplication` and accept the corresponding receiver without any additional required arguments. Generated calls are globally qualified.

Existing binders, registrars, services, transforms, configuration types, and transport registrations continue to use their normal override rules. Explicit static `IRegistrar` implementations are supported. `RegisterOnce<TRegistrar>(configuration)` records a registrar after successful registration, allowing failed registration to be retried.

Register custom persistence or shared services on an associated container before startup. Defaults are supplied only when resolution has no matching application registration, preserving closed, open generic, and conditional registrations. In-memory persistence lasts for the application's lifetime. Container verification and distinct gRPC listener setup run before the server starts.