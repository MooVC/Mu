namespace Mu.Modelling.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Mutates the <see cref="TAggregate"/>, resulting in a <see langword="new"/> instance of the aggregate with the changes applied.
/// </summary>
/// <typeparam name="TAggregate">The type of the <see cref="Aggregate"/> to which the mutation is applied.</typeparam>
/// <typeparam name="TFact">The <see cref="Fact"/> type associated with the change that has occurred.</typeparam>
public interface ITransform<TAggregate, in TFact>
    where TAggregate : Aggregate
    where TFact : Fact
{
    TAggregate Apply(TAggregate aggregate, TFact fact);
}