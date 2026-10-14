namespace Muify.Semantics.CompilationExtensionTests;

using Mu.Modelling;
using Identifier = MooVC.Syntax.Identifier;

public sealed partial class WhenParseFeatureModelIsCalled
{
    [Test]
    public async Task GivenInheritedPrivateInitializersThenOnlyTheRequestsPublicAndPrivateInitializerPayloadsAreCatalogued()
    {
        // Arrange
        const string privatePayload = "PrivatePayload";
        const string publicPayload = "PublicPayload";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public abstract record Causal
                {
                    public global::System.Guid Identity { get; private init; }

                    public global::System.DateTimeOffset Proposed { get; private init; }
                }

                public sealed partial record Register : Causal
                {
                    public string PrivatePayload { get; private init; } = string.Empty;

                    public string PublicPayload { get; init; } = string.Empty;
                }
            }
            """;
        string[] expected = [privatePayload, publicPayload];

        // Act
        Feature result = GetFeature(source);

        // Assert
        _ = await Assert.That(result.Parameters.Select(parameter => parameter.Name.ToSnippet(Identifier.Options.Pascal).ToString()).SequenceEqual(expected)).IsTrue();
    }
}