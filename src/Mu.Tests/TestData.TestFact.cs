namespace Mu.Testing;

using Mu.Modelling.Behavior;

public static partial class TestData
{
    public sealed record TestFact
        : Fact<TestAggregate>,
          IConvertFrom<TestFact, TestCreational>,
          IConvertFrom<TestFact, TestMutation>,
          IConvertFrom<TestFact, TestTransitional>
    {
        public TestFact(int value = FactValue)
        {
            Value = value;
        }

        public TestFact(Guid identity, DateTimeOffset proposed, int value = FactValue)
            : base(identity, proposed)
        {
            Value = value;
        }

        public int Value { get; init; }

        public static implicit operator TestFact(TestCreational subject)
        {
            return new TestFact(subject.Value);
        }

        public static implicit operator TestFact(TestMutation subject)
        {
            return new TestFact(subject.Value);
        }

        public static implicit operator TestFact(TestTransitional subject)
        {
            return new TestFact(subject.Value);
        }
    }
}