namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenImplicitOperatorFromStringIsCalled
{
    private const string CreationalValue = "Creational";

    [Test]
    public void GivenValueThenRoundTripsSuccessfully()
    {
        // Arrange
        string value = CreationalValue;

        // Act
        Mutational.Kind subject = value;
        string result = subject;

        // Assert
        result.ShouldBe(value);
        (subject == value).ShouldBeTrue();
        subject.Equals(value).ShouldBeTrue();
    }
}