namespace Mu.Testing;

using Mu.Modelling.Behavior;

public static partial class TestData
{
    public sealed record TestCreational
        : Creational<TestAggregate>
    {
        public TestCreational(int value = FactValue)
        {
            Value = value;
        }

        public TestCreational(Guid identity, DateTimeOffset proposed, int value = FactValue)
            : base(identity, proposed)
        {
            Value = value;
        }

        public int Value { get; init; }
    }
}