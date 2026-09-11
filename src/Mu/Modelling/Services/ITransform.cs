namespace Mu.Modelling.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Mutates the <see cref="TAggregate"/>, resulting in a <see langword="new"/> instance of the aggregate with the changes applied.
/// </summary>
public interface ITransform
{
    Aggregate Apply(Aggregate aggregate, Fact fact);
}