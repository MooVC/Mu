namespace Muify.Service.GenerateConfigurationWhenFeatureVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAFeatureWhenOptionsAreMissingThenOptionsAreGenerated(bool isPartial)
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Register.RegisterOptions";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record RegisterOptions
                : global::Mu.Configuration.Options;
            """;

        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.HasBase(false).IsPartial(isPartial));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenPartialOptionsWhenNoBaseIsDefinedThenTheBaseIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            partial record RegisterOptions
                : global::Mu.Configuration.Options;
            """;

        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Poco options = Poco.Undefined
            .IsPartial(true)
            .WithCharacteristics(characteristics => characteristics.IsClass(true).IsRecord(true));

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenOptionsWhenNotPartialThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Poco options = Poco.Undefined.WithCharacteristics(characteristics => characteristics.IsClass(true).IsRecord(true));
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenOptionsWhenAnIncompatibleBaseIsDefinedThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Poco options = Poco.Undefined
            .HasBase(true)
            .IsPartial(true)
            .WithCharacteristics(characteristics => characteristics.IsClass(true).IsRecord(true));

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(false, true)]
    [Arguments(true, false)]
    public async Task GivenPartialOptionsWhenNotARecordClassThenNothingIsGenerated(bool isClass, bool isRecord)
    {
        // Arrange
        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Poco options = Poco.Undefined
            .IsPartial(true)
            .WithCharacteristics(characteristics => characteristics.IsClass(isClass).IsRecord(isRecord));

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenOptionsWhenAlreadyDerivedThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Poco options = Poco.Undefined.HasBase(true);
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateConfigurationWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}