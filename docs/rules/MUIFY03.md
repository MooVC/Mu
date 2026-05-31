# MUIFY03 - Unit Type Not Supported

Unit is only intended to be placed on records. Classes should model entities or components instead of domain units.

## Cause

A type annotated with `[Unit<TIdentity>]` is not a record.

## How To Fix Violations

Convert the type to a record, or remove the unit annotation from types that are not domain units.

## Example

The following code produces `MUIFY03` because `Wheel` is a class:

```csharp
using Muify.Domain;

namespace Testing.Wheel;

[Unit<int>]
public sealed class Wheel
{
}
```

The following code is valid:

```csharp
using Muify.Domain;

namespace Testing.Wheel;

[Unit<int>]
public sealed record Wheel;
```