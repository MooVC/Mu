# MUIFY05 - Aggregate or Feature Constructor Constraint Not Satisfied

Positional aggregate, unit, and feature records must satisfy the `new()` constraint. This rule is an error and is enabled by default.

The rule applies to records that:

- Derive from `Mu.Modelling.State.Aggregate` or carry `[Muify.Domain.Unit<TIdentity>]`.
- Derive from `Mu.Modelling.Behavior.UseCase`, including `Creational<TAggregate>`, `Transitional<TAggregate, TIdentity>`, and `Query<TAggregate>`.
- Carry `[Muify.Service.Creational<TFact>]`, `[Muify.Service.Transitional<TFact>]`, or `[Muify.Service.NonMutational]`, including features whose base type has not yet been generated.

## Cause

A positional aggregate, unit, or feature record cannot be used as a type argument constrained by `new()`. The record must be non-abstract and declare a public parameterless constructor. Optional positional parameters and `params` parameters do not provide a parameterless constructor.

The rule recognizes indirect aggregate and feature inheritance and combines partial declarations, including attributes, bases, and constructors declared in another part of the record. Records without positional parameters are outside the scope of this rule. Unrelated types or attributes with matching short names do not trigger the rule.

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

## Feature Example

The same diagnostic applies to a positional feature such as `Open`:

```csharp
namespace Testing.Account.Open;

using Muify.Service;

[Creational<Opened>]
public sealed partial record Open(Owner Owner);
```

To keep the positional constructor, add a public parameterless constructor with an appropriate initial value:

```csharp
[Creational<Opened>]
public sealed partial record Open(Owner Owner)
{
    public Open() : this(Owner.Unspecified)
    {
    }
}
```

This satisfies `MUIFY05`. It does not enable automatic JSON constructor generation: that generator leaves records with explicit instance constructors unchanged. To use generated JSON constructors, declare the payload as initialized properties without explicit constructors; see [Feature JSON constructors](../feature-constructors.md).