namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Mu.Composition;
using Mu.Serialization;
using ProtoBuf.Meta;
using SimpleInjector;
using SimpleInjector.Lifestyles;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    [Arguments("class")]
    [Arguments("record")]
    public async Task GivenAUnitWithGeneratedAndCustomBindersThenItsRegistrarAppliesTheBinders(string kind)
    {
        // Arrange
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car;

            public sealed partial record Car : Mu.Modelling.State.Aggregate
            {
                public Details Details { get; init; } = new(string.Empty);

                public Owner Owner { get; init; } = new(string.Empty);
            }

            public sealed partial record Details(string Value);

            public sealed {{kind}} Owner : Mu.Serialization.IBinder
            {
                public Owner(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public static ProtoBuf.Meta.RuntimeTypeModel Bind(ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var metadata = model.Add(typeof(Owner), false);
                    metadata.UseConstructor = false;
                    metadata.Add(1, nameof(Name));

                    return model;
                }
            }
            """;

        Assembly assembly = await CompileRegistrar(source, AssemblyName);
        Type registrar = assembly.GetType($"{AssemblyName}.Car", throwOnError: true)!;
        Type details = assembly.GetType($"{AssemblyName}.Details", throwOnError: true)!;
        Type owner = assembly.GetType($"{AssemblyName}.Owner", throwOnError: true)!;

        // Act
        ApplyRegistrar(registrar);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(registrar)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(owner)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default[owner].GetFields()).HasSingleItem();
    }

    [Test]
    public async Task GivenAFeatureWithNestedPayloadBindersThenItsRegistrarAppliesTheBinders()
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.Search";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Search
            {
                public sealed partial record Search
                {
                    public Search()
                    {
                    }

                    public Details Details { get; init; } = new(string.Empty);

                    public Extras.Details Extra { get; init; } = new(string.Empty);

                    public sealed record Result(string Value);
                }

                public sealed partial record Details(string Value);
            }

            namespace MooVC.Testing.Mechanics.Car.Search.Extras
            {
                public sealed partial record Details(string Value);
            }
            """;

        Assembly assembly = await CompileRegistrar(source, featureAssemblyName);
        Type registrar = assembly.GetType($"{featureAssemblyName}.Search", throwOnError: true)!;
        Type details = assembly.GetType($"{featureAssemblyName}.Details", throwOnError: true)!;
        Type extra = assembly.GetType($"{featureAssemblyName}.Extras.Details", throwOnError: true)!;

        // Act
        ApplyRegistrar(registrar);

        // Assert
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(registrar)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(details)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(extra)).IsTrue();
        _ = await Assert.That(RuntimeTypeModel.Default[details].GetFields()).HasSingleItem();
        _ = await Assert.That(RuntimeTypeModel.Default[extra].GetFields()).HasSingleItem();
    }

    private static void ApplyRegistrar(Type registrar)
    {
        using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        _ = RuntimeTypeModel.Default.AddMu();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        MethodInfo method = registrar.GetMethod(nameof(IRegistrar.Register))!;

        _ = method.Invoke(default, [configuration, container]);
    }

    private static async Task<Assembly> CompileRegistrar(string source, string assemblyName)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        using var stream = new MemoryStream();
        var emitted = generated.Emit(stream);

        _ = await Assert.That(emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();

        return Assembly.Load(stream.ToArray());
    }
}