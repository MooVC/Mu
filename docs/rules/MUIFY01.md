# MUIFY01 - Type Not Supported

Identity is only intended to be placed on properties declared by a class, representing a Domain Entity. Records and structs should not declare identity properties.

## Cause

A member annotated with `[Identity]` is not a property on a non-record class.

## How To Fix Violations

Move the identity annotation to a property declared by a class, or model the type as a class when it needs an identity.

## Example

The following code produces `MUIFY01` because `Wheel` is a record:

```csharp
using Muify.Domain;

public sealed record Wheel
{
    [Identity]
    public int Id { get; init; }
}
```

The following code is valid:

```csharp
using Muify.Domain;

public sealed class Wheel
{
    [Identity]
    public int Id { get; set; }
}
```