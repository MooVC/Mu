namespace Mu.Modelling.MutationalTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string FactNameValue = "FactName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var fact = new Name(FactNameValue);
        Mutational subject = ModellingTestData.CreateMutational(fact: fact);

        // Act
        string result = subject.ToString();

        // Assert
        await Assert.That(result.Contains(nameof(Mutational))).IsTrue();
        await Assert.That(result.Contains(FactNameValue)).IsTrue();
    }
}