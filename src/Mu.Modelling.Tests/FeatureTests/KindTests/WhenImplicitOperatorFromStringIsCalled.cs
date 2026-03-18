namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenImplicitOperatorFromStringIsCalled
{
    private const string MutationalValue = "Mutational";

    [Test]
    public async Task GivenValueThenRoundTripsSuccessfully()
    {
        // Arrange
        string value = MutationalValue;

        // Act
        Feature.Kind subject = value;
        string result = subject;

        // Assert
        _ = await Assert.That(result).IsEqualTo(value);
        _ = await Assert.That(subject == value).IsTrue();
        _ = await Assert.That(subject.Equals(value)).IsTrue();
    }
}