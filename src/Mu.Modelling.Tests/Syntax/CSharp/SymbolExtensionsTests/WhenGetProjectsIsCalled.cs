namespace Mu.Modelling.Syntax.CSharp.SymbolExtensionsTests;

using MooVC.Syntax;
using MooVC.Syntax.CSharp;

public sealed class WhenGetProjectsIsCalled
{
    private const string AreaNameValue = "Area";
    private const string CompanyNameValue = "Company";
    private const string ModelNameValue = "Model";
    private const string UnitNameValue = "Current";

    [Test]
    [Arguments(CompanyNameValue, "Company.Model.Area.Other")]
    [Arguments("", "Model.Area.Other")]
    public async Task GivenCompanyAndModelNamesThenUnitDependenciesUseTheCorrectOffsets(string company, string expected)
    {
        // Arrange
        Symbol[] symbols = [(Name: "Value", Qualifier: expected)];
        Name companyName = company;
        Qualifier[] projects = [expected];

        // Act
        IEnumerable<Qualifier> result = symbols.GetProjects(companyName, ModelNameValue, AreaNameValue, UnitNameValue);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(projects);
    }

    [Test]
    public async Task GivenNoModelNameThenNoDependenciesAreReturned()
    {
        // Arrange
        Symbol[] symbols = [(Name: "Value", Qualifier: "Company.Model.Extra.One.Area.Other")];

        // Act
        IEnumerable<Qualifier> result = symbols.GetProjects(CompanyNameValue);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}