namespace Mu.Testing;

using Mu.Modelling.State;

public static partial class TestData
{
    public sealed record TestAggregate(int Value)
        : Aggregate
    {
        public TestAggregate()
            : this(DefaultValue)
        {
        }
    }
}