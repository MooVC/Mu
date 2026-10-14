namespace Mu.Composition.WebApplicationExtensionsTests;

using Microsoft.AspNetCore.Builder;
using SimpleInjector;

public sealed class WhenGetMuContainerIsCalled
{
    [Test]
    public async Task GivenUnconfiguredApplicationThenNoContainerIsReturned()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication app = builder.Build();

        // Act
        Container? result = app.GetMuContainer();

        // Assert
        _ = await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GivenAssociatedApplicationThenItsContainerIsReturnedWithoutLockingRegistrations()
    {
        // Arrange
        const string markerValue = "A registration after passive container lookup";
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication app = builder.BuildMu(out Container expected);

        // Act
        Container? result = app.GetMuContainer();
        expected.RegisterInstance(new Marker(markerValue));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(expected);
        _ = await Assert.That(app.GetMuContainer()).IsSameReferenceAs(expected);
        _ = await Assert.That(expected.IsLocked).IsFalse();
    }

    [Test]
    public async Task GivenNullApplicationThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The application associated with the Mu container must be provided.";
        WebApplication app = default!;

        // Act
        Exception? exception = Capture(() => _ = app.GetMuContainer());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(app));
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