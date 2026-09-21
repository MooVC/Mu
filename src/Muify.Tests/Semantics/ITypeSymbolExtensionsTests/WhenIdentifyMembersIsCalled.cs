namespace Muify.Semantics.ITypeSymbolExtensionsTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed class WhenIdentifyMembersIsCalled
{
    [Test]
    public async Task GivenCancelledTokenThenThrowsWithResourceMessageAndOriginalToken()
    {
        // Arrange
        const string assemblyName = "CancellationTests";
        const string typeName = "Example.Parent";
        const string expectedMessage = "The operation to identify members was canceled.";
        const string source = """
            namespace Example
            {
                public class Parent
                {
                    public Child Child { get; set; }
                }

                public class Child
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        INamedTypeSymbol symbol = compilation.GetTypeByMetadataName(typeName)!;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Act
        OperationCanceledException? exception = null;

        try
        {
            symbol.IdentifyMembers(out _, out _, cancellation.Token);
        }
        catch (OperationCanceledException canceled)
        {
            exception = canceled;
        }

        // Assert
        _ = await Assert.That(exception).IsNotNull();
        _ = await Assert.That(exception!.Message).IsEqualTo(expectedMessage);
        _ = await Assert.That(exception.CancellationToken).IsEqualTo(cancellation.Token);
    }
}