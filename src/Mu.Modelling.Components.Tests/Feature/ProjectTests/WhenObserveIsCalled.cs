namespace Mu.Modelling.Components.Feature.ProjectTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAFeatureThenProjectDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Project();

        const string content = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <Description>Registers a Car within the Mechanics System</Description>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="src/MooVC.Testing.Mechanics.Car/MooVC.Testing.Mechanics.Car.csproj" />
              </ItemGroup>
            </Project>
            """;

        var expected = new File(content, "csproj", "MooVC.Testing.Mechanics.Car.Register", "src/MooVC.Testing.Mechanics.Car.Register/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Register, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}