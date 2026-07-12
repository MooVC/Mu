namespace Mu.Testing;

using Mu.Modelling.Behavior;

public static partial class TestData
{
    public sealed record AlternateFact
        : Fact<TestAggregate>;
}