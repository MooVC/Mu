# MUIFY04 - Unit Type Name Mismatch

Unit records must be named after the last segment of their containing namespace. This keeps the generated model aligned with the assembly and namespace convention used to discover domain units.

## Cause

A record annotated with `[Unit<TIdentity>]` has a name that does not match the last namespace segment.

## How To Fix Violations

Rename the record or move it into a namespace whose final segment matches the record name.

## Example

The following code produces `MUIFY04` because the namespace ends with `Car` but the unit record is named `Wheel`:

```csharp
using Muify.Domain;

namespace Testing.Car;

[Unit<int>]
public sealed record Wheel;
```

The following code is valid:

```csharp
using Muify.Domain;

namespace Testing.Wheel;

[Unit<int>]
public sealed record Wheel;
```