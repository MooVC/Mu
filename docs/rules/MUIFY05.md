# MUIFY05 - Aggregate Constructor Constraint Not Satisfied

Positional records deriving from `Mu.Modelling.State.Aggregate` or annotated with `[Muify.Domain.Unit<TIdentity>]` must satisfy the `new()` constraint. This rule is an error and is enabled by default.

## Cause

A positional aggregate or unit record cannot be used as a type argument constrained by `new()`. The record must be non-abstract and declare a public parameterless constructor. Optional positional parameters and `params` parameters do not provide a parameterless constructor.

The rule recognizes indirect aggregate inheritance and combines partial declarations, including constructors declared in another part of the record. Records without positional parameters are outside the scope of this rule.

If the record has required fields or properties, including inherited members, its public parameterless constructor must be annotated with `[SetsRequiredMembers]` to satisfy `new()`.

## How To Fix Violations

Add a public parameterless constructor that delegates to the positional constructor with appropriate initial values, or replace the positional parameters with initialized properties. The record must also be non-abstract. When required members are present, initialize them in the constructor and annotate it with `[System.Diagnostics.CodeAnalysis.SetsRequiredMembers]`.

## Example

The following code produces `MUIFY05` because `Wheel` only has a constructor that takes `Size`:

```csharp
namespace Testing.Wheel;

using Mu.Modelling.State;

public sealed record Wheel(int Size) : Aggregate;
```

The rule also applies to unit records whose aggregate base is generated:

```csharp
namespace Testing.Wheel;

using Muify.Domain;

[Unit<int>]
public sealed partial record Wheel(int Size);
```

The following positional record is valid because it also has a public parameterless constructor:

```csharp
namespace Testing.Wheel;

using Mu.Modelling.State;

public sealed record Wheel(int Size) : Aggregate
{
    public Wheel() : this(0)
    {
    }
}
```

Alternatively, use an initialized property:

```csharp
namespace Testing.Wheel;

using Muify.Domain;

[Unit<int>]
public sealed partial record Wheel
{
    public int Size { get; init; } = 0;
}
```