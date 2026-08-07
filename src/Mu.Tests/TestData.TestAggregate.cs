namespace Mu.Testing;

using Mu.Modelling.State;
using ProtoBuf;

public static partial class TestData
{
    [ProtoContract]
    public sealed record TestAggregate([property: ProtoMember(1, Name = nameof(TestAggregate.Value))] int Value)
        : Aggregate
    {
        public TestAggregate()
            : this(DefaultValue)
        {
        }
    }
}