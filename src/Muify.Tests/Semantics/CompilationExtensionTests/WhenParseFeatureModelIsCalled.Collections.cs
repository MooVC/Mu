namespace Muify.Semantics.CompilationExtensionTests;

using MooVC.Syntax.CSharp;
using Mu.Modelling;

public sealed partial class WhenParseFeatureModelIsCalled
{
    [Test]
    public async Task GivenMutationalComponentsThenOnlyConcreteMatchingImplementationsAreDiscovered()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using Mu.Modelling.Behavior;
                using Mu.Modelling.Integrity;
                using Mu.Modelling.Services;
                using SimpleInjector;

                [Muify.Service.Creational<Registered>]
                public sealed record Register : Creational<Car>;

                public sealed record Registered : Fact;

                public abstract class InvariantBase : IInvariant<Car, Register>;

                public sealed class PlainInvariant : InvariantBase;

                public sealed class GenericInvariant<TValue> : InvariantBase;

                public sealed class OtherInvariant : IInvariant<Car, object>;

                public sealed class OtherAggregateInvariant : IInvariant<object, Register>;

                public abstract class TransformBase : ITransform<Car, Registered>;

                public sealed class PlainTransform : TransformBase;

                public sealed class GenericTransform<TValue> : TransformBase;

                public sealed class OtherTransform : ITransform<Car, object>;

                public sealed class OtherAggregateTransform : ITransform<object, Registered>;

                public static class Rules
                {
                    public sealed class Check : InvariantBase, IRegistrar
                    {
                        public static Container Register(IConfiguration configuration, Container container) => container;
                    }
                }
            }

            namespace MooVC.Testing.Mechanics.Car.Register.Custom
            {
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using Mu.Modelling.Services;
                using SimpleInjector;

                public sealed class CustomTransform : ITransform<Car, Registered>, IRegistrar
                {
                    public static Container Register(IConfiguration configuration, Container container) => container;
                }
            }
            """;
        Qualification[] expectedInvariants =
        [
            (Name: "PlainInvariant", Qualifier: AssemblyName),
            (Name: "Rules.Check", Qualifier: AssemblyName),
        ];
        Qualification[] expectedTransforms =
        [
            (Name: "CustomTransform", Qualifier: $"{AssemblyName}.Custom"),
            (Name: "PlainTransform", Qualifier: AssemblyName),
        ];
        Qualification[] expectedRegistrars =
        [
            (Name: "CustomTransform", Qualifier: $"{AssemblyName}.Custom"),
            (Name: "Rules.Check", Qualifier: AssemblyName),
        ];

        // Act
        Feature.Semantics result = GetFeature(source).Metadata;

        // Assert
        _ = await Assert.That(result.Invariants).IsEquivalentTo(expectedInvariants);
        _ = await Assert.That(result.Transforms).IsEquivalentTo(expectedTransforms);
        _ = await Assert.That(result.Registrars).IsEquivalentTo(expectedRegistrars);
    }
}