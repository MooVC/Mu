namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenImplicitOperatorFromStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";

    [Test]
    public async Task GivenValueThenRoundTripsSuccessfully()
    {
        // Arrange
        string value = ReadStoreValue;

        // Act
        NonMutational.Kinds subject = value;
        string result = subject;

        // Assert
        _ = await Assert.That(result).IsEqualTo(value);
        _ = await Assert.That(subject == value).IsTrue();
        _ = await Assert.That(subject.Equals(value)).IsTrue();
    }
}