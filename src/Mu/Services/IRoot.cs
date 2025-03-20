namespace Mu.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

public interface IRoot<TAggregate, in TMutation>
    where TAggregate : Aggregate
    where TMutation : Mutational
{
    Result<TAggregate> Apply(TAggregate aggregate, TMutation mutation);
}