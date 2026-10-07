namespace Muify.Semantics.CompilationExtensionTests;

using MooVC.Syntax.CSharp;

public sealed partial class WhenParseFeatureModelIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenRootImplementationsWhenAMatchingConcreteRootIsPresentThenItIsSelected(bool hasRoot)
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                using Mu.Modelling.Behavior;
                using Mu.Modelling.Services;

                [Muify.Service.Creational<Registered>]
                public sealed record Register : Creational<Car>;

                public sealed record Registered : Fact;

                public abstract class RootBase : IRoot<Car, Register>;

                public sealed class GenericRoot<TValue> : RootBase;

                public sealed class OtherRoot : IRoot<Car, object>;

                public sealed class OtherAggregateRoot : IRoot<object, Register>;
            }
            """;
        const string customSource = """
            namespace MooVC.Testing.Mechanics.Car.Register.Custom
            {
                public static class Policies
                {
                    public sealed class Root : RootBase;
                }
            }
            """;
        Qualification expected = hasRoot
            ? (Name: "Policies.Root", Qualifier: $"{AssemblyName}.Custom")
            : Qualification.Unnamed;

        // Act
        Qualification result = GetFeature(hasRoot ? source + customSource : source).Metadata.Root;

        // Assert
        _ = await Assert.That(result).IsEqualTo(expected);
    }
}