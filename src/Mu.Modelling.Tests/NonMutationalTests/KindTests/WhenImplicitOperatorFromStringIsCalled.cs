namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenImplicitOperatorFromStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";

    [Test]
    public async Task GivenValueThenRoundTripsSuccessfully()
    {
        // Arrange
        string value = ReadStoreValue;

        // Act
        NonMutational.Kind subject = value;
        string result = subject;

        // Assert
        await Assert.That(result).IsEqualTo(value);
        await Assert.That(subject == value).IsTrue();
        await Assert.That(subject.Equals(value)).IsTrue();
    }
}