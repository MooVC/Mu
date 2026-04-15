#if NET10_0_OR_GREATER
namespace Mu.Modelling.Components.Domain.ProjectTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAnAreaThenProjectDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Project();

        const string content = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <Description>Represents a Mechanics Shop</Description>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Mu" />
                <PackageReference Include="Muify">
                  <PrivateAssets>all</PrivateAssets>
                  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
                </PackageReference>
              </ItemGroup>
            </Project>
            """;

        var expected = new File(content, "csproj", "MooVC.Testing.Mechanics", "src/MooVC.Testing.Mechanics/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Mechanics, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}
#endif