namespace Mu.Communications.PubSub.ReflectionPolicyDirectoryTests;

using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenFindIsCalled
{
    [Test]
    public async Task GivenRegisteredPoliciesThenReturnsPolicyWrapper()
    {
        // Arrange
        Event<TestData.TestFact, Guid> @event = TestData.CreateEvent();
        var policy = new TestData.TestPolicy();
        IServiceProvider provider = Substitute.For<IServiceProvider>();
        IServiceScope scope = Substitute.For<IServiceScope>();
        var subject = new ReflectionPolicyDirectory();

        _ = scope
            .ServiceProvider
            .Returns(provider);

        _ = provider
            .GetService(typeof(IEnumerable<IPolicy<TestData.TestFact, Guid>>))
            .Returns(new IPolicy<TestData.TestFact, Guid>[] { policy });

        // Act
        IPolicy? result = subject.Find(@event, scope);

        // Assert
        _ = await Assert.That(result).IsNotNull();

        await result!.Apply(@event, CancellationToken.None);

        _ = await Assert.That(policy.Events).IsEquivalentTo(new[] { @event });
    }

    [Test]
    public async Task GivenNoRegisteredPoliciesThenReturnsNull()
    {
        // Arrange
        IServiceProvider provider = Substitute.For<IServiceProvider>();
        IServiceScope scope = Substitute.For<IServiceScope>();
        var subject = new ReflectionPolicyDirectory();

        _ = scope
            .ServiceProvider
            .Returns(provider);

        // Act
        IPolicy? result = subject.Find(TestData.CreateEvent(), scope);

        // Assert
        _ = await Assert.That(result).IsNull();
    }
}