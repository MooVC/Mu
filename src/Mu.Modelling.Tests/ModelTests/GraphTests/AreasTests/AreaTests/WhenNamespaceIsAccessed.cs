namespace Mu.Modelling.ModelTests.GraphTests.AreasTests.AreaTests;

using MooVC.Syntax;
using AreaGraph = Mu.Modelling.Model.Graph.Areas.Area;
using AreasGraph = Mu.Modelling.Model.Graph.Areas;
using FeatureGraph = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Features.Feature;
using FeaturesGraph = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Features;
using UnitGraph = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit;
using UnitsGraph = Mu.Modelling.Model.Graph.Areas.Area.Units;

public sealed class WhenNamespaceIsAccessed
{
    private const string AreaNameValue = "Area";
    private const string CompanyNameValue = "Company";
    private const string FeatureNameValue = "Feature";
    private const string ModelNameValue = "Model";
    private const string UnitNameValue = "Unit";

    [Test]
    [Arguments("", "Company.Model.Area", "Company.Model.Area.Unit", "Company.Model.Area.Unit.Feature")]
    [Arguments("Extra", "Company.Model.Extra.Area", "Company.Model.Extra.Area.Unit", "Company.Model.Extra.Area.Unit.Feature")]
    [Arguments("Extra.One", "Company.Model.Extra.One.Area", "Company.Model.Extra.One.Area.Unit", "Company.Model.Extra.One.Area.Unit.Feature")]
    public async Task GivenModelScopeThenAllGraphNamespacesIncludeTheScope(
        string scope,
        string areaNamespace,
        string unitNamespace,
        string featureNamespace)
    {
        // Arrange
        Model model = Model.Undefined
            .Defines(area => area
                .Named(AreaNameValue)
                .ResponsibleFor(unit => unit
                    .Named(UnitNameValue)
                    .Featuring(feature => feature.Named(FeatureNameValue).IsNonMutational())))
            .For(CompanyNameValue)
            .Named(ModelNameValue)
            .ScopedWithin(scope);
        var areas = new AreasGraph(model, model.Areas);
        var area = new AreaGraph(areas, 0, model, model.Areas[0]);
        var units = new UnitsGraph(area, model, area._Value.Units);
        var unit = new UnitGraph(units, 0, model, units._Value[0]);
        var features = new FeaturesGraph(unit, model, unit._Value.Features);
        var feature = new FeatureGraph(features, 0, model, features._Value[0]);

        // Act
        Qualifier areaResult = area.Namespace;
        Qualifier unitResult = unit.Namespace;
        Qualifier featureResult = feature.Namespace;

        // Assert
        _ = await Assert.That(areaResult).IsEqualTo((Qualifier)areaNamespace);
        _ = await Assert.That(unitResult).IsEqualTo((Qualifier)unitNamespace);
        _ = await Assert.That(featureResult).IsEqualTo((Qualifier)featureNamespace);
        _ = await Assert.That(area.ProjectName).IsEqualTo(areaNamespace);
        _ = await Assert.That(unit.ProjectName).IsEqualTo(unitNamespace);
        _ = await Assert.That(unit.KernelName).IsEqualTo(areaNamespace);
        _ = await Assert.That(feature.ProjectName).IsEqualTo(featureNamespace);
        _ = await Assert.That(feature.DomainName).IsEqualTo(unitNamespace);
    }
}