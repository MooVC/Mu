namespace Mu.Testing;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

public static partial class TestData
{
    public sealed record TestTransitional
        : Transitional<TestAggregate, Guid>
    {
        public TestTransitional(Reference<Guid> target, int value = FactValue)
            : base(target)
        {
            Value = value;
        }

        public TestTransitional(Guid identity, DateTimeOffset proposed, Reference<Guid> target, int value = FactValue)
            : base(identity, proposed, target)
        {
            Value = value;
        }

        public int Value { get; init; }
    }
}