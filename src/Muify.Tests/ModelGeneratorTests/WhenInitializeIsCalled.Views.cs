namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Mu;
using Mu.Serialization;
using ProtoBuf.Meta;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    public async Task GivenANonMutationalFeatureWithOutputOnlyReferencesThenItsGrpcResponseRoundtrips()
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.ReadView";
        const string expectedOwner = "Alex";
        const string expectedSummary = "Available cars";
        const string expectedValue = "Roadster";
        const int expectedItems = 1;
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.ReadView
            {
                public sealed partial record ReadView : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public ReadView()
                    {
                    }

                    public sealed partial record Result(string Summary, Details[] Details);
                }

                public sealed partial record Details(string Value, Owner Owner);

                public sealed partial record Owner(string Name);
            }
            """;
        Assembly assembly = await CompileRegistrar(source, featureAssemblyName);
        Type request = assembly.GetType($"{featureAssemblyName}.ReadView", throwOnError: true)!;
        Type view = assembly.GetType($"{featureAssemblyName}.ReadView+Result", throwOnError: true)!;
        Type details = assembly.GetType($"{featureAssemblyName}.Details", throwOnError: true)!;
        Type owner = assembly.GetType($"{featureAssemblyName}.Owner", throwOnError: true)!;
        object expectedOwnerValue = Activator.CreateInstance(owner, expectedOwner)!;
        object expectedDetails = Activator.CreateInstance(details, expectedValue, expectedOwnerValue)!;
        Array expectedCollection = Array.CreateInstance(details, expectedItems);
        expectedCollection.SetValue(expectedDetails, index: 0);
        object expectedView = Activator.CreateInstance(view, expectedSummary, expectedCollection)!;
        Type response = typeof(Result<>).MakeGenericType(view);
        object expected = response.GetMethod("op_Implicit", [view])!.Invoke(default, [expectedView])!;
        using var buffer = new MemoryStream();

        // Act
        ApplyRegistrar(request);
        RuntimeTypeModel.Default.Serialize(buffer, expected);
        buffer.Position = 0;
        object actual = RuntimeTypeModel.Default.Deserialize(buffer, value: null, type: response);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(view)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(owner)).IsTrue();
        _ = await Assert.That((bool)response.GetProperty(nameof(Result<object>.IsSuccessful))!.GetValue(actual)!).IsTrue();
        object actualView = response.GetProperty(nameof(Result<object>.Value))!.GetValue(actual)!;
        _ = await Assert.That(view.GetProperty("Summary")!.GetValue(actualView)).IsEqualTo(expectedSummary);
        var actualCollection = (Array)view.GetProperty("Details")!.GetValue(actualView)!;
        _ = await Assert.That(actualCollection.Length).IsEqualTo(expectedItems);
        object actualDetails = actualCollection.GetValue(index: 0)!;
        _ = await Assert.That(details.GetProperty("Value")!.GetValue(actualDetails)).IsEqualTo(expectedValue);
        object actualOwner = details.GetProperty("Owner")!.GetValue(actualDetails)!;
        _ = await Assert.That(owner.GetProperty("Name")!.GetValue(actualOwner)).IsEqualTo(expectedOwner);
    }

    [Test]
    public async Task GivenANonMutationalFeatureWithCustomViewBindersThenItsRegistrarPreservesTheirBindings()
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.ReadCustomView";
        const int expectedDetailsTag = 13;
        const int expectedResultTag = 11;
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.ReadCustomView
            {
                public sealed partial record ReadCustomView : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public ReadCustomView()
                    {
                    }

                    public sealed record Result(Details Details) : global::Mu.Serialization.IBinder
                    {
                        public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                        {
                            var metadata = model.Add(typeof(Result), false);
                            metadata.UseConstructor = false;
                            metadata.Add({{expectedResultTag}}, nameof(Details));

                            return model;
                        }
                    }
                }

                public sealed record Details(string Value) : global::Mu.Serialization.IBinder
                {
                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        var metadata = model.Add(typeof(Details), false);
                        metadata.UseConstructor = false;
                        metadata.Add({{expectedDetailsTag}}, nameof(Value));

                        return model;
                    }
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(source, featureAssemblyName);
        Type request = assembly.GetType($"{featureAssemblyName}.ReadCustomView", throwOnError: true)!;
        Type view = assembly.GetType($"{featureAssemblyName}.ReadCustomView+Result", throwOnError: true)!;
        Type details = assembly.GetType($"{featureAssemblyName}.Details", throwOnError: true)!;

        // Act
        ApplyRegistrar(request);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(view)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        ValueMember resultField = await Assert.That(RuntimeTypeModel.Default[view].GetFields()).HasSingleItem();
        ValueMember detailsField = await Assert.That(RuntimeTypeModel.Default[details].GetFields()).HasSingleItem();
        _ = await Assert.That(resultField.FieldNumber).IsEqualTo(expectedResultTag);
        _ = await Assert.That(detailsField.FieldNumber).IsEqualTo(expectedDetailsTag);
    }

    [Test]
    [Arguments("ReadPartialGuard", true)]
    [Arguments("ReadCompleteGuard", false)]
    public async Task GivenANonMutationalFeatureWithNonPartialViewTypesThenOnlyEligibleBindersAreGenerated(string featureName, bool isPartialResult)
    {
        // Arrange
        string featureAssemblyName = $"MooVC.Testing.Mechanics.Car.{featureName}";
        string partialModifier = isPartialResult ? "partial " : string.Empty;
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace {{featureAssemblyName}}
            {
                public sealed partial record {{featureName}} : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public {{featureName}}()
                    {
                    }

                    public sealed {{partialModifier}}record Result(Details Details, Owner Owner);
                }

                public sealed partial record Details(string Value);

                public sealed record Owner(string Name);
            }
            """;
        Assembly assembly = await CompileRegistrar(source, featureAssemblyName);
        Type request = assembly.GetType($"{featureAssemblyName}.{featureName}", throwOnError: true)!;
        Type view = assembly.GetType($"{featureAssemblyName}.{featureName}+Result", throwOnError: true)!;
        Type details = assembly.GetType($"{featureAssemblyName}.Details", throwOnError: true)!;
        Type owner = assembly.GetType($"{featureAssemblyName}.Owner", throwOnError: true)!;

        // Act
        ApplyRegistrar(request);

        // Assert
        _ = await Assert.That(typeof(IBinder).IsAssignableFrom(view)).IsEqualTo(isPartialResult);
        _ = await Assert.That(view.GetMethod(nameof(IBinder.Bind), BindingFlags.Public | BindingFlags.Static) is not null).IsEqualTo(isPartialResult);
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        _ = await Assert.That(typeof(IBinder).IsAssignableFrom(owner)).IsFalse();
        _ = await Assert.That(owner.GetMethod(nameof(IBinder.Bind), BindingFlags.Public | BindingFlags.Static)).IsNull();
    }

    [Test]
    [Arguments("ReadDomainFirst", true)]
    [Arguments("ReadFeatureFirst", false)]
    public async Task GivenANonMutationalFeatureReturningItsDomainThenBothRegistrarsPreserveTheDomainBindings(string featureName, bool registerDomainFirst)
    {
        // Arrange
        const int expectedDetailsTag = 3;
        const int expectedPropositionsTag = 1;
        const int expectedResultTag = 4;
        const int expectedRevisionTag = 2;
        const int expectedValueTag = 1;
        string featureAssemblyName = $"MooVC.Testing.Mechanics.Car.{featureName}";
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed partial record Car : Mu.Modelling.State.Aggregate
                {
                    public Details Details { get; init; } = new(string.Empty);
                }

                public sealed partial record Details(string Value);
            }

            namespace {{featureAssemblyName}}
            {
                public sealed partial record {{featureName}} : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public {{featureName}}()
                    {
                    }

                    public sealed partial record Result(global::MooVC.Testing.Mechanics.Car.Car[] Cars);
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(source, featureAssemblyName);
        Type aggregate = assembly.GetType($"{AssemblyName}.Car", throwOnError: true)!;
        Type details = assembly.GetType($"{AssemblyName}.Details", throwOnError: true)!;
        Type request = assembly.GetType($"{featureAssemblyName}.{featureName}", throwOnError: true)!;
        Type view = assembly.GetType($"{featureAssemblyName}.{featureName}+Result", throwOnError: true)!;
        (int Tag, string Name)[] expectedAggregateFields =
        [
            (expectedPropositionsTag, "Propositions"),
            (expectedRevisionTag, "Revision"),
            (expectedDetailsTag, "Details"),
        ];

        // Act
        ApplyRegistrar(registerDomainFirst ? aggregate : request);
        ApplyRegistrar(registerDomainFirst ? request : aggregate);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(aggregate)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(view)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default[aggregate].GetFields().Select(field => (Tag: field.FieldNumber, field.Name)))
            .IsEquivalentTo(expectedAggregateFields);
        ValueMember detailsField = await Assert.That(RuntimeTypeModel.Default[details].GetFields()).HasSingleItem();
        _ = await Assert.That(detailsField.FieldNumber).IsEqualTo(expectedValueTag);
        _ = await Assert.That(detailsField.Name).IsEqualTo("Value");
        ValueMember resultField = await Assert.That(RuntimeTypeModel.Default[view].GetFields()).HasSingleItem();
        _ = await Assert.That(resultField.FieldNumber).IsEqualTo(expectedResultTag);
        _ = await Assert.That(resultField.Name).IsEqualTo("Cars");
    }

    [Test]
    public async Task GivenANonMutationalFeatureWithAnEmptyPartialViewThenItsBinderIsRegistered()
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.ReadEmptyView";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.ReadEmptyView
            {
                public sealed partial record ReadEmptyView : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public ReadEmptyView()
                    {
                    }

                    public sealed partial record Result;
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(source, featureAssemblyName);
        Type request = assembly.GetType($"{featureAssemblyName}.ReadEmptyView", throwOnError: true)!;
        Type view = assembly.GetType($"{featureAssemblyName}.ReadEmptyView+Result", throwOnError: true)!;

        // Act
        ApplyRegistrar(request);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(view)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default[view].GetFields()).IsEmpty();
    }
}