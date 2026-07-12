namespace Mu.Testing;

using Mu.Modelling;
using Mu.Modelling.Behavior;

public static partial class TestData
{
    public sealed record TestMutation
        : Mutational
    {
        public TestMutation(int value = FactValue)
        {
            Value = value;
        }

        public TestMutation(Guid identity, DateTimeOffset proposed, int value = FactValue)
            : base(identity, proposed)
        {
            Value = value;
        }

        public override Representation Model => typeof(TestAggregate);

        public int Value { get; init; }
    }
}