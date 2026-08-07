namespace Mu.Testing;

using Mu.Modelling;
using Mu.Modelling.Behavior;
using ProtoBuf;

public static partial class TestData
{
    [ProtoContract(SkipConstructor = true)]
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

        [ProtoMember(1, Name = nameof(Value))]
        public int Value { get; init; }
    }
}