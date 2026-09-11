namespace Mu.Modelling.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Mutates the <see cref="TAggregate"/>, resulting in a <see langword="new"/> instance of the aggregate with the changes applied.
/// </summary>
/// <typeparam name="TAggregate">The type of the <see cref="Aggregate"/> to which the mutation is applied.</typeparam>
public interface ITransform<TAggregate>
    where TAggregate : Aggregate
{
    TAggregate Apply(TAggregate aggregate, Fact fact);
}