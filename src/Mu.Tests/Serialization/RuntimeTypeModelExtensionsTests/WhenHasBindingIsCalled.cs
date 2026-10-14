namespace Mu.Serialization.RuntimeTypeModelExtensionsTests;

using ProtoBuf.Meta;

public sealed class WhenHasBindingIsCalled
{
    [Test]
    public async Task GivenARegisteredTypeThenItsBindingIsDetected()
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create();
        _ = model.Add(typeof(View), applyDefaultBehaviour: false);

        // Act
        bool result = model.HasBinding<View>();

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenAnUnregisteredRecordThenItsContractIsNotInferred()
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create();

        // Act
        bool result = model.HasBinding<View>();

        // Assert
        _ = await Assert.That(result).IsFalse();
        _ = await Assert.That(model.GetTypes().Cast<MetaType>()).IsEmpty();
    }

    [Test]
    public async Task GivenAnotherRegisteredTypeThenTheRequestedBindingRemainsMissing()
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create();
        MetaType expected = model.Add(typeof(OtherView), applyDefaultBehaviour: false);

        // Act
        bool result = model.HasBinding<View>();

        // Assert
        _ = await Assert.That(result).IsFalse();
        MetaType definition = await Assert.That(model.GetTypes().Cast<MetaType>()).HasSingleItem();
        _ = await Assert.That(definition).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task GivenANullModelThenThrowsArgumentNullExceptionWithAMessage()
    {
        // Arrange
        const string expectedMessage = "The runtime type model to inspect must be provided.";
        RuntimeTypeModel model = default!;

        // Act
        Exception? exception = Capture(() => _ = model.HasBinding<View>());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(model));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();

            return default;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    public readonly record struct OtherView(string Value);

    public readonly record struct View(string Value);
}