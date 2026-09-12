# Changelog
All notable changes to Mu will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Initial Release
- Generate unit protobuf binders and identity allocator registrars through the model generation pipeline.
- Discover referenced domain units and generate feature facts with their declared payloads, namespaces, and internal constructors.
- Build generated feature facts with the shared syntax engine, including imports, constructors, properties, and conversion operators.
- Populate aggregate properties so generated transforms apply matching fact payloads.
- Generate creational, transitional, and query base records for partial features without an existing use-case base.
- Add error MUIFY05 when a positional record deriving from Aggregate or annotated with Unit<> does not satisfy the new() constraint.