# Executable features and hosts

A Mu application consists of a domain project containing its aggregate and a feature project containing its request and optional result. Muify generates the protobuf binders, registrars, services, configuration, and application composition.

Project names can contain additional namespace nodes between the model and area:

- Unit: `Company.Model.[extra nodes...].Area.Unit`
- Feature: `Company.Model.[extra nodes...].Area.Unit.Feature`
- Host: `Company.Model.[extra nodes...].Area.Unit.Host`

Muify identifies units and features from the record whose name matches the final node in its namespace. A unit record derives from `Aggregate<TIdentity>` or has `Muify.Domain.Unit<TIdentity>`. A feature record derives from `Causal` or has a `Muify.Service.Creational<TFact>`, `Transitional<TFact>`, or `NonMutational` attribute. Hosts are identified by the final `.Host` namespace node and require no marker record, class, or attribute. Ordinary projects without a unit or feature record are excluded unless they use the host convention.

Additional nodes are retained in `Model.Scope`, set through `ScopedWithin`, and included in generated namespaces. For example, `MooVC.Testing.Payments.Internal.Modelling.Account.Open` has company `MooVC`, model `Testing`, scope `Payments.Internal`, area `Modelling`, unit `Account`, and feature `Open`.

Generated feature project files include `<OutputType>Exe</OutputType>`. Set it explicitly in handwritten feature executable projects:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
</PropertyGroup>
```

The number of name nodes no longer selects executable output in MSBuild. Muify's props default projects ending in `.Host` to `OutputType=Exe`, while an explicit `OutputType` takes precedence. Library projects keep their composition extensions without a generated entry point.

If an aliased host supplies its `.Host` `AssemblyName` or `RootNamespace` only inside the project file, also set `<OutputType>Exe</OutputType>` there. Executable defaults are selected during the early SDK imports; names supplied earlier in `Directory.Build.props` are detected automatically.

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

Feature registrars also apply protobuf binders for the nested `{Feature.Name}.Result` record and its local output DTOs, including collection elements and recursively referenced view types. Result/view binders use payload fields starting at 4 and include empty partial results. A generated binder requires the type and every enclosing declaration to be partial; a custom `IBinder` implementation suppresses generation and is still registered, including implementations in referenced assemblies. Domain-owned types reuse their existing or generated domain binder. Shared type registrations check whether the runtime model already defines the type, allowing hosted and remote features to share domain and view contracts.

Shared model libraries can reference Muify without following Unit, Feature, or Host conventions. Their public and internal partial classes, records, and structs receive binders even when no feature references them. Muify generates a public `Binder : Mu.Serialization.IBinder` in the library's root namespace to apply those binders, including binders for internal types and existing custom contracts. Nested types require partial enclosing declarations; open generic, static, ref-like, file-local, and inaccessible declarations are excluded. Both host and standalone feature build extensions invoke referenced assembly binders through `BindOnce` before composing features, once per runtime model, with each contract bound only if it is absent. A public custom type named `Binder` that implements `IBinder` suppresses the generated default assembly binder. The assembly binder can live in a different namespace from its assembly name, including a custom root namespace.

## Host project

A host has a namespace ending in `.Host`, with its build extension named after the preceding node. Both `MooVC.Testing.Modelling.Account.Host` and `MooVC.Testing.Payments.Internal.Modelling.Account.Host` generate `BuildAccount()`. Short hosts such as `My.Project.Host` remain supported and generate `BuildProject()`. The configured `RootNamespace` is used when provided; otherwise the assembly name is used.

Reference the feature projects from the host and include Mu and the Muify analyzer. No host class or record is required. Muify discovers referenced feature requests with callable application build methods, sorts them by qualification, and generates a host entry point and composition extensions. Referenced features can have any supported number of name nodes. Ordinary referenced libraries are excluded. Each hosted feature's unit registrar runs once per container, including when several features reference the same domain.

For the account host, a custom program's composition is simply `builder.BuildAccount()`. The application extensions also support manual chaining such as `builder.BuildMu().BuildOpen().BuildClose()` before startup.

## Overrides

An included `Program.cs`, top-level statements, or an existing entry point suppresses the generated program. A compatible public build method suppresses only that receiver's generated extension; application and builder methods can be replaced independently. Classic extension methods and C# 14 extension blocks are supported. Build methods return `WebApplication` and accept the corresponding receiver without any additional required arguments. Generated calls are globally qualified.

Existing binders, registrars, services, transforms, configuration types, and transport registrations continue to use their normal override rules. Explicit static `IRegistrar` implementations are supported. `RegisterOnce<TRegistrar>(configuration)` records a registrar after successful registration, allowing failed registration to be retried.

Register custom persistence or shared services on an associated container before startup. Defaults are supplied only when resolution has no matching application registration, preserving closed, open generic, and conditional registrations. In-memory persistence lasts for the application's lifetime. Container verification and distinct gRPC listener setup run before the server starts.