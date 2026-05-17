# MUIFY02 - Duplicate Identity Attribute

Identity can only be placed on one property for a given type. Multiple identity annotations make the identity ambiguous.

## Cause

A type declares more than one property annotated with `[Identity]`.

## How To Fix Violations

Keep `[Identity]` on the single property that uniquely identifies the type and remove the other identity annotations.

## Example

The following code produces `MUIFY02` because both properties are annotated as identities:

```csharp
using Muify.Domain;

public sealed class Wheel
{
    [Identity]
    public int Id { get; set; }

    [Identity]
    public int Number { get; set; }
}
```

The following code is valid:

```csharp
using Muify.Domain;

public sealed class Wheel
{
    [Identity]
    public int Id { get; set; }

    public int Number { get; set; }
}
```