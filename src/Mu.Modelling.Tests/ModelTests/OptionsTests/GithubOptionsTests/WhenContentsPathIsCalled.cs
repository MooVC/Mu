namespace Mu.Modelling.ModelTests.OptionsTests.GithubOptionsTests;

using System.Text.Json;
using static Mu.Modelling.Options;

public sealed class WhenContentsPathIsCalled
{
    private const string ApiBaseAddress = "https://api.github.com/";
    private const string Owner = "owner";
    private const string Reference = "main";
    private const string RelativePath = "source/File.cs";
    private const string Repository = "repository";

    [Test]
    public async Task GivenGithubOptionsThenContentsPathsAreReturned()
    {
        // Arrange
        var subject = new GithubOptions(ApiBaseAddress, JsonSerializerOptions.Default, Owner, Repository, Reference, string.Empty);

        // Act
        string repositoryPath = subject.ContentsPath(string.Empty);
        string filePath = subject.ContentsPath(RelativePath);

        // Assert
        _ = await Assert.That(repositoryPath).IsEqualTo($"repos/{Owner}/{Repository}/contents?ref={Reference}");
        _ = await Assert.That(filePath).IsEqualTo($"repos/{Owner}/{Repository}/contents/{RelativePath}?ref={Reference}");
    }
}