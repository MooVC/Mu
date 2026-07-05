namespace Mu.Testing;

using Mu.Modelling.Behavior;

public sealed record TestQuery
    : Query<MuTestData.TestAggregate>
{
    public TestQuery()
    {
    }

    public TestQuery(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}