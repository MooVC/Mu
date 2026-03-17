namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenImplicitOperatorFromStringIsCalled
{
    private const string CreationalValue = "Creational";

    [Test]
    public async Task GivenValueThenRoundTripsSuccessfully()
    {
        // Arrange
        string value = CreationalValue;

        // Act
        Mutational.Kind subject = value;
        string result = subject;

        // Assert
        await Assert.That(result).IsEqualTo(value);
        await Assert.That(subject == value).IsTrue();
        await Assert.That(subject.Equals(value)).IsTrue();
    }
}