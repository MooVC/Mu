namespace Mu.Composition.WebApplicationBuilderExtensionsTests;

using Microsoft.AspNetCore.Builder;
using SimpleInjector;

public sealed class WhenGetMuContainerIsCalled
{
    [Test]
    public async Task GivenUnconfiguredBuilderThenNoContainerIsReturnedAndServicesAreUnchanged()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        int expectedCount = builder.Services.Count;

        // Act
        Container? result = builder.GetMuContainer();

        // Assert
        _ = await Assert.That(result).IsNull();
        _ = await Assert.That(builder.Services.Count).IsEqualTo(expectedCount);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAssociatedContainerThenItIsReturnedWithoutLockingRegistrations(bool useHostBuilder)
    {
        // Arrange
        const string markerValue = "A registration after passive container lookup";
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        Container expected;

        if (useHostBuilder)
        {
            expected = builder.AddMu();
        }
        else
        {
            _ = builder.Services.AddMu(out expected);
        }

        using (expected)
        {
            int expectedCount = builder.Services.Count;

            // Act
            Container? result = builder.GetMuContainer();
            expected.RegisterInstance(new Marker(markerValue));

            // Assert
            _ = await Assert.That(result).IsSameReferenceAs(expected);
            _ = await Assert.That(expected.IsLocked).IsFalse();
            _ = await Assert.That(builder.Services.Count).IsEqualTo(expectedCount);
            _ = await Assert.That(builder.GetMuContainer()).IsSameReferenceAs(expected);
        }
    }

    [Test]
    public async Task GivenBuiltApplicationThenItsContainerIsReturned()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication host = builder.BuildMu(out Container expected);

        // Act
        Container? result = builder.GetMuContainer();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task GivenNullBuilderThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The builder associated with the Mu container must be provided.";
        WebApplicationBuilder builder = default!;

        // Act
        Exception? exception = Capture(() => _ = builder.GetMuContainer());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(builder));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();

            return default;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    public sealed record Marker(string Value);
}