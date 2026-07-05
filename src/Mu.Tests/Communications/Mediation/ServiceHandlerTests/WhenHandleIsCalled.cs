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
        var useCase = new MuTestData.TestMutation(MuTestData.Identity, MuTestData.ProposedAt);
        var intent = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, useCase);
        IService<MuTestData.TestMutation, string> service = Substitute.For<IService<MuTestData.TestMutation, string>>();
        Result<string> serviceResult = MuTestData.ResultValue;
        using var source = new CancellationTokenSource();

        _ = service
            .Execute(useCase, source.Token)
            .Returns(Task.FromResult(serviceResult));

        var subject = new ServiceHandler<MuTestData.TestMutation, string>(service);

        // Act
        Outcome<string> result = await subject.Handle(intent, source.Token);

        // Assert
        _ = await Assert.That(result.Result).IsSameReferenceAs(serviceResult);
        _ = await Assert.That(result.Ledger.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(result.Ledger.Correlation).IsEqualTo(intent.Ledger.Correlation);
        _ = await service.Received(1).Execute(useCase, source.Token);
    }
}