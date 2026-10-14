namespace Mu.Modelling.ModelTests;

using MooVC.Syntax;

public sealed class WhenScopedWithinIsCalled
{
    private const string ScopeValue = "Extra.One";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Model original = ModellingTestData.CreateModel();
        Qualifier scope = ScopeValue;

        // Act
        Model result = original.ScopedWithin(scope);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Scope).IsEqualTo(scope);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
        _ = await Assert.That(result.Company).IsEqualTo(original.Company);
        _ = await Assert.That(result.Areas).IsEquivalentTo(original.Areas);
        _ = await Assert.That(original.Scope).IsEqualTo(Qualifier.Unqualified);
    }
}