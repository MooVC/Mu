namespace Muify.Semantics.CompilationExtensionTests;

using Mu.Modelling;

public sealed partial class WhenParseFeatureModelIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenApplicationAndBuilderBuildMethodsWhenTheirReceiversAreCompatibleOrIncompatibleThenOverrideMetadataMatches(bool hasCompatibleReceiver)
    {
        // Arrange
        string applicationReceiver = hasCompatibleReceiver ? "WebApplication" : "WebApplicationBuilder";
        string builderReceiver = hasCompatibleReceiver ? "WebApplicationBuilder" : "WebApplication";
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed partial record Register;

                public static class WebApplicationExtensions
                {
                    public static global::Microsoft.AspNetCore.Builder.WebApplication BuildRegister(
                        this global::Microsoft.AspNetCore.Builder.{{applicationReceiver}} app)
                    {
                        return default!;
                    }
                }

                public static class WebApplicationBuilderExtensions
                {
                    extension(global::Microsoft.AspNetCore.Builder.{{builderReceiver}} builder)
                    {
                        public global::Microsoft.AspNetCore.Builder.WebApplication BuildRegister()
                        {
                            return default!;
                        }
                    }
                }
            }
            """;

        // Act
        Feature.Semantics result = GetFeature(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasBuildExtension).IsEqualTo(hasCompatibleReceiver);
        _ = await Assert.That(result.HasBuilderBuildExtension).IsEqualTo(hasCompatibleReceiver);
    }
}