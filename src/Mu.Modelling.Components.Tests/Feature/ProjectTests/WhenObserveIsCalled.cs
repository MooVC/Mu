namespace Mu.Modelling.Components.Feature.ProjectTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MooVC.Modelling;
using FeatureGraph = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Features.Feature;

public sealed class WhenObserveIsCalled
{
    private const string AreaName = "Mechanics";
    private const string CompanyName = "MooVC";
    private const string FeatureDescription = "Registers a Car within the Mechanics System";
    private const string FeatureName = "Register";
    private const string ModelName = "Testing";
    private const string UnitName = "Car";

    [Test]
    [Arguments("", "MooVC.Testing.Mechanics.Car")]
    [Arguments("Extra.One", "MooVC.Testing.Extra.One.Mechanics.Car")]
    public async Task GivenAFeatureScopeThenExecutableProjectDefinitionIsReturned(string scope, string domainName)
    {
        // Arrange
        var visitor = new Project();
        Model model = Model.Undefined
            .Defines(area => area
                .Named(AreaName)
                .ResponsibleFor(unit => unit
                    .Named(UnitName)
                    .Featuring(feature => feature
                        .DescribedAs(FeatureDescription)
                        .IsNonMutational()
                        .Named(FeatureName))))
            .For(CompanyName)
            .Named(ModelName)
            .ScopedWithin(scope);
        var services = new ServiceCollection();
        _ = services.AddSingleton<IModelVisitor<FeatureGraph, File>>(visitor);
        _ = services.AddModelNavigator();
        using ServiceProvider provider = services.BuildServiceProvider();
        IModelNavigator navigator = provider.GetRequiredService<IModelNavigator>();

        string content = $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <Description>{{FeatureDescription}}</Description>
              </PropertyGroup>
              <PropertyGroup>
                <OutputType>Exe</OutputType>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="src/{{domainName}}/{{domainName}}.csproj" />
              </ItemGroup>
            </Project>
            """;

        string projectName = $"{domainName}.{FeatureName}";
        var expected = new File(content, "csproj", projectName, $"src/{projectName}/");

        // Act
        IAsyncEnumerable<File> results = navigator.Navigate<File>(model, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}