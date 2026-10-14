namespace Mu.Modelling.Syntax.CSharp.SymbolExtensionsTests;

using MooVC.Syntax;
using MooVC.Syntax.CSharp;

public sealed class WhenGetScopedProjectsIsCalled
{
    private const string AreaNameValue = "Area";
    private const string CompanyNameValue = "Company";
    private const string ModelNameValue = "Model";
    private const string ScopeValue = "Extra.One";
    private const string UnitNameValue = "Current";

    [Test]
    [Arguments("", "Company.Model.Area")]
    [Arguments("Extra", "Company.Model.Extra.Area")]
    [Arguments(ScopeValue, "Company.Model.Extra.One.Area")]
    public async Task GivenModelScopeThenAreaDependenciesIncludeAllModelNodes(string scope, string expected)
    {
        // Arrange
        Symbol[] symbols = [(Name: "Value", Qualifier: expected)];
        Qualifier[] projects = [expected];

        // Act
        IEnumerable<Qualifier> result = symbols.GetScopedProjects(CompanyNameValue, (Qualifier)scope, ModelNameValue, AreaNameValue);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(projects);
    }

    [Test]
    public async Task GivenScopedUnitThenOnlyDistinctDependenciesInTheSameModelScopeAreReturned()
    {
        // Arrange
        const string dependency = "Company.Model.Extra.One.Area.Other";
        Symbol[] symbols =
        [
            (Name: "Value", Qualifier: dependency),
            (Name: "NestedValue", Qualifier: $"{dependency}.Nested"),
            (Name: "Local", Qualifier: "Company.Model.Extra.One.Area.Current"),
            (Name: "OtherScope", Qualifier: "Company.Model.Extra.Two.Area.Other"),
            (Name: "OtherModel", Qualifier: "Company.Other.Extra.One.Area.Other"),
            (Name: "OtherCompany", Qualifier: "Other.Model.Extra.One.Area.Other"),
            (Name: "OtherArea", Qualifier: "Company.Model.Extra.One.OtherArea.Other"),
        ];
        Qualifier[] expected = [dependency];

        // Act
        IEnumerable<Qualifier> result = symbols.GetScopedProjects(CompanyNameValue, (Qualifier)ScopeValue, ModelNameValue, AreaNameValue, UnitNameValue);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
    }

    [Test]
    public async Task GivenIncompleteModelAndAreaPrefixesThenOnlyTheAreaProjectIsReturned()
    {
        // Arrange
        const string modelPrefix = "Company.Model.Extra.One";
        const string areaPrefix = "Company.Model.Extra.One.Area";
        Symbol[] symbols =
        [
            (Name: "ModelValue", Qualifier: modelPrefix),
            (Name: "AreaValue", Qualifier: areaPrefix),
        ];
        Qualifier[] expected = [areaPrefix];

        // Act
        IEnumerable<Qualifier> result = symbols.GetScopedProjects(CompanyNameValue, (Qualifier)ScopeValue, ModelNameValue, AreaNameValue, UnitNameValue);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
    }
}