# Changelog
All notable changes to Mu will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Include the namespace in generated source hint names to avoid collisions between types with the same name in different namespaces.
- Add `MediationOptions` and `RegisterMediator` container extensions accepting options or configuration and registering a scoped in-memory mediator by default.
- Add `RegisterAuditor` container extensions accepting audit options or configuration and registering a singleton in-memory auditor by default.
- Add `InMemoryAuditor` with thread-safe message capture, outcome and failure recording, and immutable audit entry snapshots for development and testing.
- Register a scoped custom root for mutational features when a matching implementation exists, falling back to the default root through generated feature registrars.
- Register scoped invariant and transform collections through generated feature registrars, including generated default transforms and custom registrars on nested and child-namespace implementations.
- Apply aggregate and component protobuf binders through generated unit registrars and request and payload binders through generated feature registrars, assuming each model is bound exactly once and supporting custom binders on non-partial models.
- Register the application service provider, scoped in-memory mediator, singleton scribe and scope manager, and logging through `AddMu()`, including hosts created with `BuildMu()`.
- Populate visitor metadata from source symbols so missing component binders, equality/comparison members, and feature service/gRPC artifacts are generated while existing implementations are preserved. Generated services use the mediator and runtime result contracts; gRPC clients accept a timeout directly.
- Register all implemented model visitors, including component, feature, and feature-reference binders and service/gRPC generators, with distinct generated source names.
- Generate gRPC clients with trace headers, deadlines, and cancellation forwarding; add request guards to generated services and nest gRPC implementations under Service.Grpc.
- Replace ThrowIf checks with Ardalis guards and resource-backed messages, including generated identifier conversions and cancellation checks that preserve the original token.
- Migrate Mu runtime extension methods to C# 14 extension blocks while preserving behavior and keeping Muify and Mu.Modelling compatible with C# 7.3.
- Move remaining guard and precondition messages into class-level resource files while preserving exception types, parameter names, and message text.
- Initial Release
- Add `IScribe` and `Scribe` to manage ambient ledger causation and correlation through disposable use case scopes, restoring parent context and supporting initialization from an incoming ledger with localized Ardalis guards.
- Generate unit protobuf binders and identity allocator registrars through the model generation pipeline.
- Discover referenced domain units and generate feature facts with their declared payloads, namespaces, and internal constructors.
- Build generated feature facts with the shared syntax engine, including imports, constructors, properties, and conversion operators.
- Populate aggregate properties so generated transforms apply matching fact payloads.
- Generate creational, transitional, and query base records for partial features without an existing use-case base.
- Generate JSON constructors for partial features without explicit constructors, restoring their payload, causal identity, proposed time, and transitional target.
- Generate public parameterless constructors alongside feature JSON constructors, preserving payload initializers and initializing causal metadata through the feature base.
- Implement feature service contract and service visitors with the shared syntax builder, including asynchronous handler delegation within audit and tracing scopes.
- Implement nested gRPC service contracts and services with the shared syntax builder, including operation metadata, call cancellation, and incoming trace ledger scopes.
- Add error MUIFY05 when a positional record deriving from Aggregate or annotated with Unit<> does not satisfy the new() constraint.
- Extend MUIFY05 to positional Creational, Transitional, and Query features, including features identified by their attributes before base generation.