namespace Mu.Testing;

using Mu.Modelling.Behavior;
using Mu.Modelling.Services;

public static partial class TestData
{
    public sealed class TestTransform
        : ITransform<TestAggregate, Fact>,
          ITransform<TestAggregate, TestFact>
    {
        public IList<TestFact> Facts { get; } = [];

        public TestAggregate Apply(TestAggregate aggregate, TestFact fact)
        {
            Facts.Add(fact);

            return aggregate with
            {
                Value = aggregate.Value + fact.Value,
            };
        }

        TestAggregate ITransform<TestAggregate, Fact>.Apply(TestAggregate aggregate, Fact fact)
        {
            return Apply(aggregate, (TestFact)fact);
        }
    }
}