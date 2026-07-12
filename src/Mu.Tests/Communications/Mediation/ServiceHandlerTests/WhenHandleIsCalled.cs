namespace Mu.Communications.Mediation.ServiceHandlerTests;

using Mu.Communications.Messaging;
using Mu.Modelling.Services;
using Mu.Testing;

public sealed class WhenHandleIsCalled
{
    [Test]
    public async Task GivenIntentThenDelegatesToServiceAndReturnsOutcome()
    {
        // Arrange
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var intent = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, useCase);
        IService<TestData.TestMutation, string> service = Substitute.For<IService<TestData.TestMutation, string>>();
        Result<string> serviceResult = TestData.ResultValue;
        using var source = new CancellationTokenSource();

        _ = service
            .Execute(useCase, source.Token)
            .Returns(Task.FromResult(serviceResult));

        var subject = new ServiceHandler<TestData.TestMutation, string>(service);

        // Act
        Outcome<string> result = await subject.Handle(intent, source.Token);

        // Assert
        _ = await Assert.That(result.Result).IsSameReferenceAs(serviceResult);
        _ = await Assert.That(result.Ledger.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(result.Ledger.Correlation).IsEqualTo(intent.Ledger.Correlation);
        _ = await service.Received(1).Execute(useCase, source.Token);
    }
}