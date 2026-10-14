namespace Mu.Modelling.ModelTests.GraphTests.AreasTests.AreaTests;

using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using AreaGraph = Mu.Modelling.Model.Graph.Areas.Area;
using AreasGraph = Mu.Modelling.Model.Graph.Areas;
using UnitGraph = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit;
using UnitsGraph = Mu.Modelling.Model.Graph.Areas.Area.Units;

public sealed class WhenProjectsIsAccessed
{
    private const string AreaNamespace = "Company.Model.Extra.One.Area";
    private const string AttributeProject = "Company.Model.Extra.One.Area.AttributeSource";
    private const string ComponentProject = "Company.Model.Extra.One.Area.ComponentSource";
    private const string ParameterProject = "Company.Model.Extra.One.Area.ParameterSource";
    private const string ResultProject = "Company.Model.Extra.One.Area.ResultSource";
    private const string ViewProject = "Company.Model.Extra.One.Area.ViewSource";

    [Test]
    public async Task GivenScopedModelThenAreaAndUnitDependenciesUseTheCompletePrefix()
    {
        // Arrange
        Model model = Model.Undefined
            .Defines(area => area
                .Named("Area")
                .Owns(component => component
                    .Named("Shared")
                    .AttributedWith((Name: "Value", Type: (Symbol)(Name: "Shared", Qualifier: AttributeProject))))
                .ResponsibleFor(unit => unit
                    .Named("Unit")
                    .AttributedWith((Name: "Value", Type: (Symbol)(Name: "Value", Qualifier: AttributeProject)))
                    .Owns(component => component
                        .Named("Component")
                        .AttributedWith((Name: "Value", Type: (Symbol)(Name: "Component", Qualifier: ComponentProject))))
                    .Featuring(feature => feature
                        .Named("Feature")
                        .IsNonMutational()
                        .Using((Name: "Value", Type: (Symbol)(Name: "Parameter", Qualifier: ParameterProject)))
                        .Returning(result => result
                            .Named("Value")
                            .OfType((Name: "Result", Qualifier: ResultProject))))
                    .SeenAs(view => view
                        .Named("View")
                        .AttributedWith((Name: "Value", Type: (Symbol)(Name: "View", Qualifier: ViewProject))))))
            .For("Company")
            .Named("Model")
            .ScopedWithin("Extra.One");
        var areas = new AreasGraph(model, model.Areas);
        var area = new AreaGraph(areas, 0, model, model.Areas[0]);
        var units = new UnitsGraph(area, model, area._Value.Units);
        var unit = new UnitGraph(units, 0, model, units._Value[0]);
        Qualifier[] expectedArea = [AreaNamespace];
        Qualifier[] expectedUnit = [AttributeProject, ComponentProject, ParameterProject, ResultProject, ViewProject];

        // Act
        IEnumerable<Qualifier> areaResult = area.Projects;
        IEnumerable<Qualifier> unitResult = unit.Projects;

        // Assert
        _ = await Assert.That(areaResult).IsEquivalentTo(expectedArea);
        _ = await Assert.That(unitResult).IsEquivalentTo(expectedUnit);
    }
}