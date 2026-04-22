namespace Muify.ModelGeneratorTests;

using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mu.Modelling;

public sealed class WhenGetModelIsCalled
{
    private const string AreaName = "Billing";

    private const string CompanyName = "Contoso";

    private const string DomainAssemblyName = "Contoso.Sales.Billing.Invoice";

    private const string DomainIdentitySource = """
        using System;

        namespace Muify.Domain;

        [AttributeUsage(AttributeTargets.Property)]
        public sealed class IdentityAttribute : Attribute
        {
        }
        """;

    private const string DomainModelSource = """
        using System;
        using Muify.Domain;

        namespace Contoso.Sales.Billing.Invoice;

        public sealed record Invoice(Customer Customer, Address Address);

        public sealed class Customer
        {
            [Identity]
            public Guid Id { get; init; }
        }

        public sealed record Address(string Line1);
        """;

    private const string FeatureAssemblyName = "Contoso.Sales.Billing.Invoice.GetInvoice";

    private const string FeatureName = "GetInvoice";

    private const string FeatureSource = """
        namespace Contoso.Sales.Billing.Invoice.GetInvoice;

        public sealed record GetInvoice
        {
            public sealed record Receipt(string Number);
        }
        """;

    private const string ModelName = "Sales";

    private const string UnitName = "Invoice";

    [Test]
    public async Task GivenFeatureAssemblyThenMapsRequestAndResultRecords()
    {
        // Arrange
        Compilation compilation = CreateCompilation(FeatureAssemblyName, FeatureSource);

        // Act
        Model model = InvokeGetModel(compilation);

        // Assert
        _ = await Assert.That(model.Company.ToString()).IsEqualTo(CompanyName);
        _ = await Assert.That(model.Name.ToString()).IsEqualTo(ModelName);
        _ = await Assert.That(model.Areas.Length).IsEqualTo(1);
        _ = await Assert.That(model.Areas[0].Name.ToString()).IsEqualTo(AreaName);
        _ = await Assert.That(model.Areas[0].Units.Length).IsEqualTo(1);
        _ = await Assert.That(model.Areas[0].Units[0].Name.ToString()).IsEqualTo(UnitName);
        _ = await Assert.That(model.Areas[0].Units[0].Features.Length).IsEqualTo(1);
        _ = await Assert.That(model.Areas[0].Units[0].Features[0].Name.ToString()).IsEqualTo(FeatureName);
        _ = await Assert.That(model.Areas[0].Units[0].Features[0].Results.Length).IsEqualTo(1);
        _ = await Assert.That(model.Areas[0].Units[0].Features[0].Results[0].Name.ToString()).IsEqualTo("Receipt");
    }

    [Test]
    public async Task GivenDomainAssemblyThenMapsAggregateComponents()
    {
        // Arrange
        Compilation compilation = CreateCompilation(DomainAssemblyName, DomainIdentitySource, DomainModelSource);

        // Act
        Model model = InvokeGetModel(compilation);
        Component customer = model.Areas[0].Components.Single(component => component.Name.ToString() == "Customer");
        Component address = model.Areas[0].Components.Single(component => component.Name.ToString() == "Address");

        // Assert
        _ = await Assert.That(model.Areas[0].Units[0].Name.ToString()).IsEqualTo(UnitName);
        _ = await Assert.That(model.Areas[0].Components.Length).IsEqualTo(2);
        _ = await Assert.That(customer.Identifier.IsUndefined).IsFalse();
        _ = await Assert.That(customer.Identifier.Name.ToString()).IsEqualTo("Id");
        _ = await Assert.That(address.Identifier.IsUndefined).IsTrue();
    }

    private static Compilation CreateCompilation(string assemblyName, params string[] sources)
    {
        SyntaxTree[] syntaxTrees = sources
            .Select(source => CSharpSyntaxTree.ParseText(source))
            .ToArray();

        MetadataReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Model).Assembly.Location),
        ];

        return CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static Model InvokeGetModel(Compilation compilation)
    {
        MethodInfo method = typeof(ModelGenerator).GetMethod("GetModel", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("GetModel method was not found.");

        object? result = method.Invoke(null, [compilation, CancellationToken.None]);

        return result as Model
            ?? throw new InvalidOperationException("GetModel did not return a model.");
    }
}