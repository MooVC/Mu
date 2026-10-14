namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mu.Serialization;
using ProtoBuf.Meta;

public sealed partial class WhenInitializeIsCalled
{
    private const string SharedAssemblyName = "MooVC.Testing.Shared.Contracts";

    [Test]
    [Arguments("sealed partial class", "partial class")]
    [Arguments("sealed partial record", "partial record")]
    [Arguments("partial struct", "partial struct")]
    [Arguments("partial record struct", "partial record struct")]
    [Arguments("readonly partial record struct", "readonly partial record struct")]
    public async Task GivenAnOrdinaryProjectWithAPartialTypeThenItsTypeAndAssemblyBindersAreGenerated(string declaration, string expectedDeclaration)
    {
        // Arrange
        const string expectedBinderHint = "MooVC.Testing.Shared.Contracts.Details.Binder.g.cs";
        const string expectedAssemblyBinderHint = "MooVC.Testing.Shared.Contracts.Binder.g.cs";
        string source = $$"""
            namespace {{SharedAssemblyName}};

            public {{declaration}} Details
            {
                public string Value { get; init; }
            }
            """;
        string expectedBinder = $$"""
            namespace MooVC.Testing.Shared.Contracts;

            {{expectedDeclaration}} Details
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var meta = model.Add(typeof(Details), false);

                    meta.UseConstructor = false;

                    meta.Add(4, "Value");

                    return model;
                }
            }
            """;
        const string expectedAssemblyBinder = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial class Binder
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    if (!global::Mu.Serialization.RuntimeTypeModelExtensions.HasBinding<global::MooVC.Testing.Shared.Contracts.Details>(model))
                    {
                        _ = global::MooVC.Testing.Shared.Contracts.Details.Bind(model);
                    }

                    return model;
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult binder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedBinderHint)).HasSingleItem();
        GeneratedSourceResult assemblyBinder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedAssemblyBinderHint)).HasSingleItem();
        _ = await Assert.That(binder.SourceText.ToString()).IsEqualTo(expectedBinder);
        _ = await Assert.That(assemblyBinder.SourceText.ToString()).IsEqualTo(expectedAssemblyBinder);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    [Arguments("readonly partial record struct")]
    [Arguments("partial record struct")]
    public async Task GivenAnOrdinaryProjectWithANestedInternalPartialTypeThenItsBinderIsGeneratedInsideTheContainingType(string declaration)
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Shared.Contracts.Envelope.Details.Binder.g.cs";
        string source = $$"""
            namespace MooVC.Testing.Shared.Contracts;

            public {{declaration}} Envelope
            {
                internal sealed partial record Details(string Value);
            }
            """;
        string expected = $$"""
            namespace MooVC.Testing.Shared.Contracts;

            {{declaration}} Envelope
            {
                partial record Details
                    : global::Mu.Serialization.IBinder
                {
                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        var meta = model.Add(typeof(Envelope.Details), false);

                        meta.UseConstructor = false;

                        meta.Add(4, "Value");

                        return model;
                    }
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult binder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(binder.SourceText.ToString()).IsEqualTo(expected);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    [Arguments("public sealed record Details(string Value);")]
    [Arguments("public static partial class Details { }")]
    [Arguments("public ref partial struct Details { }")]
    [Arguments("file sealed partial record Details(string Value);")]
    [Arguments("public sealed partial record Details<T>(T Value);")]
    [Arguments("public sealed class Envelope { public sealed partial record Details(string Value); }")]
    [Arguments("public sealed partial class Envelope<T> { public sealed partial record Details(string Value); }")]
    [Arguments("public sealed class Envelope { private sealed partial record Details(string Value); }")]
    public async Task GivenAnOrdinaryProjectWithAnIneligibleTypeThenNoBindersAreGenerated(string declaration)
    {
        // Arrange
        string source = $$"""
            namespace {{SharedAssemblyName}};

            {{declaration}}
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources).IsEmpty();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithAPrivateNestedPartialTypeThenOnlyItsContainingTypeHasABinder()
    {
        // Arrange
        const string suppressedHint = "MooVC.Testing.Shared.Contracts.Envelope.Details.Binder.g.cs";
        const int expectedSourceCount = 2;
        const string source = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial class Envelope
            {
                private sealed partial record Details(string Value);
            }
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Length).IsEqualTo(expectedSourceCount);
        _ = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == suppressedHint)).IsEmpty();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithPartialTypesAcrossNamespacesThenItsAssemblyBinderIncludesEveryType()
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Shared.Contracts.Binder.g.cs";
        const int expectedSourceCount = 3;
        const string source = """
            namespace MooVC.Testing.Shared.Contracts
            {
                internal sealed partial record Details(string Value);
            }

            namespace MooVC.Extras.Shared
            {
                public partial struct Payload
                {
                    public int Value { get; init; }
                }
            }
            """;
        const string expected = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial class Binder
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    if (!global::Mu.Serialization.RuntimeTypeModelExtensions.HasBinding<global::MooVC.Extras.Shared.Payload>(model))
                    {
                        _ = global::MooVC.Extras.Shared.Payload.Bind(model);
                    }
                    if (!global::Mu.Serialization.RuntimeTypeModelExtensions.HasBinding<global::MooVC.Testing.Shared.Contracts.Details>(model))
                    {
                        _ = global::MooVC.Testing.Shared.Contracts.Details.Bind(model);
                    }

                    return model;
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Length).IsEqualTo(expectedSourceCount);
        GeneratedSourceResult assemblyBinder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(assemblyBinder.SourceText.ToString()).IsEqualTo(expected);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithACustomBinderThenOnlyItsAssemblyBinderIsGenerated()
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Shared.Contracts.Binder.g.cs";
        const string source = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed record Details(string Value) : Mu.Serialization.IBinder
            {
                public static ProtoBuf.Meta.RuntimeTypeModel Bind(ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    return model;
                }
            }
            """;
        const string expected = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial class Binder
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    if (!global::Mu.Serialization.RuntimeTypeModelExtensions.HasBinding<global::MooVC.Testing.Shared.Contracts.Details>(model))
                    {
                        _ = global::MooVC.Testing.Shared.Contracts.Details.Bind(model);
                    }

                    return model;
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult assemblyBinder = await Assert.That(result.GeneratedSources).HasSingleItem();
        _ = await Assert.That(assemblyBinder.HintName).IsEqualTo(expectedHint);
        _ = await Assert.That(assemblyBinder.SourceText.ToString()).IsEqualTo(expected);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithACustomAssemblyBinderThenItsBinderIsPreserved()
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Shared.Contracts.Details.Binder.g.cs";
        const string source = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial record Details(string Value);

            public sealed class Binder : Mu.Serialization.IBinder
            {
                public static ProtoBuf.Meta.RuntimeTypeModel Bind(ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    return model;
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult binder = await Assert.That(result.GeneratedSources).HasSingleItem();
        _ = await Assert.That(binder.HintName).IsEqualTo(expectedHint);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithSeveralPartialDeclarationsThenEachBinderIsGeneratedOnce()
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Shared.Contracts.Details.Binder.g.cs";
        const int expectedSourceCount = 2;
        const string source = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial class Details
            {
                public string Value { get; init; }
            }

            public sealed partial class Details;
            """;

        // Act
        GeneratorRunResult result = GenerateSharedSources(source, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Length).IsEqualTo(expectedSourceCount);
        _ = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithExistingGeneratedBindersThenNoDefinitionsAreGeneratedAgain()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Shared.Contracts;

            public sealed partial record Details(string Value);
            """;
        _ = GenerateSharedSources(source, out Compilation generated);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(generated).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources).IsEmpty();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithAnEmptyPartialTypeThenItsBinderIsRegistered()
    {
        // Arrange
        const string assemblyName = "MooVC.Testing.Shared.EmptyContracts";
        const string source = """
            namespace MooVC.Testing.Shared.EmptyContracts;

            internal sealed partial record Details;
            """;
        Assembly assembly = await CompileRegistrar(source, assemblyName);
        Type assemblyBinder = assembly.GetType($"{assemblyName}.Binder", throwOnError: true)!;
        Type details = assembly.GetType($"{assemblyName}.Details", throwOnError: true)!;

        // Act
        ApplySharedBinder(assemblyBinder);

        // Assert
        _ = await Assert.That(typeof(IBinder).IsAssignableFrom(details)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default[details].GetFields()).IsEmpty();
    }

    [Test]
    public async Task GivenAnOrdinaryProjectWithGeneratedAndCustomBindersWhenRegisteredTwiceThenItsPayloadRoundtripsWithTheOriginalBindings()
    {
        // Arrange
        const string assemblyName = "MooVC.Testing.Shared.RoundtripContracts";
        const string expectedName = "Alex";
        const string expectedValue = "Roadster";
        const int expectedCustomTag = 13;
        const int expectedGeneratedTag = 4;
        string source = $$"""
            namespace MooVC.Testing.Shared.RoundtripContracts;

            internal sealed partial record Payload(Details Details, Owner Owner);

            internal sealed partial record Details(string Value);

            public sealed record Owner(string Name) : Mu.Serialization.IBinder
            {
                public static ProtoBuf.Meta.RuntimeTypeModel Bind(ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var metadata = model.Add(typeof(Owner), false);
                    metadata.UseConstructor = false;
                    metadata.Add({{expectedCustomTag}}, nameof(Name));

                    return model;
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(source, assemblyName);
        Type assemblyBinder = assembly.GetType($"{assemblyName}.Binder", throwOnError: true)!;
        Type payload = assembly.GetType($"{assemblyName}.Payload", throwOnError: true)!;
        Type details = assembly.GetType($"{assemblyName}.Details", throwOnError: true)!;
        Type owner = assembly.GetType($"{assemblyName}.Owner", throwOnError: true)!;
        object expectedDetails = Activator.CreateInstance(details, expectedValue)!;
        object expectedOwner = Activator.CreateInstance(owner, expectedName)!;
        object expected = Activator.CreateInstance(payload, expectedDetails, expectedOwner)!;
        using var buffer = new MemoryStream();

        // Act
        ApplySharedBinder(assemblyBinder);
        ApplySharedBinder(assemblyBinder);
        RuntimeTypeModel.Default.Serialize(buffer, expected);
        buffer.Position = 0;
        object actual = RuntimeTypeModel.Default.Deserialize(buffer, value: null, type: payload);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(payload)).IsTrue();
        ValueMember detailsField = await Assert.That(RuntimeTypeModel.Default[details].GetFields()).HasSingleItem();
        ValueMember ownerField = await Assert.That(RuntimeTypeModel.Default[owner].GetFields()).HasSingleItem();
        _ = await Assert.That(detailsField.FieldNumber).IsEqualTo(expectedGeneratedTag);
        _ = await Assert.That(ownerField.FieldNumber).IsEqualTo(expectedCustomTag);
        object actualDetails = payload.GetProperty("Details")!.GetValue(actual)!;
        object actualOwner = payload.GetProperty("Owner")!.GetValue(actual)!;
        _ = await Assert.That(details.GetProperty("Value")!.GetValue(actualDetails)).IsEqualTo(expectedValue);
        _ = await Assert.That(owner.GetProperty("Name")!.GetValue(actualOwner)).IsEqualTo(expectedName);
    }

    private static void ApplySharedBinder(Type binder)
    {
        _ = RuntimeTypeModel.Default.AddMu();
        MethodInfo method = binder.GetMethod(nameof(IBinder.Bind))!;

        _ = method.Invoke(default, [RuntimeTypeModel.Default]);
    }

    private static GeneratorRunResult GenerateSharedSources(string source, out Compilation generated)
    {
        var compilation = CSharpCompilation.Create(
            SharedAssemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out generated, out _);

        return driver.GetRunResult().Results.Single();
    }
}