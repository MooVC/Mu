namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenImplicitOperatorFromStringIsCalled
{
    private const string CreationalValue = "Creational";

    [Test]
    public async Task GivenValueThenRoundTripsSuccessfully()
    {
        // Arrange
        string value = CreationalValue;

        // Act
        Mutational.Kinds subject = value;
        string result = subject;

        // Assert
        _ = await Assert.That(result).IsEqualTo(value);
        _ = await Assert.That(subject == value).IsTrue();
        _ = await Assert.That(subject.Equals(value)).IsTrue();
    }
}