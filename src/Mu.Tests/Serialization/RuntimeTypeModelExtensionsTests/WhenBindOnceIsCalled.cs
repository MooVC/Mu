namespace Mu.Serialization.RuntimeTypeModelExtensionsTests;

using ProtoBuf.Meta;

public sealed class WhenBindOnceIsCalled
{
    private const string FailureMessage = "The binder failed before configuring the model.";

    private const int FieldNumber = 1;

    [Test]
    public async Task GivenRepeatedCallsThenTheBindingIsAppliedOnceAndTheModelIsReturned()
    {
        // Arrange
        var model = RuntimeTypeModel.Create();

        // Act
        RuntimeTypeModel first = model.BindOnce<FirstBinder>();
        RuntimeTypeModel repeated = model.BindOnce<FirstBinder>();

        // Assert
        _ = await Assert.That(first).IsSameReferenceAs(model);
        _ = await Assert.That(repeated).IsSameReferenceAs(model);
        MetaType contract = await Assert.That(model.GetTypes().Cast<MetaType>()).HasSingleItem();
        _ = await Assert.That(contract.Type).IsEqualTo(typeof(FirstView));
        ValueMember field = await Assert.That(contract.GetFields()).HasSingleItem();
        _ = await Assert.That(field.FieldNumber).IsEqualTo(FieldNumber);
        _ = await Assert.That(field.Name).IsEqualTo(nameof(FirstView.Value));
    }

    [Test]
    public async Task GivenDifferentBindersThenEachBindingIsAppliedOnce()
    {
        // Arrange
        var model = RuntimeTypeModel.Create();
        Type[] expectedContracts = [typeof(FirstView), typeof(SecondView)];

        // Act
        _ = model.BindOnce<FirstBinder>();
        _ = model.BindOnce<SecondBinder>();
        _ = model.BindOnce<SecondBinder>();
        _ = model.BindOnce<FirstBinder>();

        // Assert
        _ = await Assert.That(model.GetTypes().Cast<MetaType>().Select(contract => contract.Type)).IsEquivalentTo(expectedContracts);
        _ = await Assert.That(model[typeof(FirstView)].GetFields()).HasSingleItem();
        _ = await Assert.That(model[typeof(SecondView)].GetFields()).HasSingleItem();
    }

    [Test]
    public async Task GivenDifferentModelsThenTheBinderConfiguresEachModel()
    {
        // Arrange
        var first = RuntimeTypeModel.Create();
        var second = RuntimeTypeModel.Create();

        // Act
        RuntimeTypeModel firstResult = first.BindOnce<FirstBinder>();
        RuntimeTypeModel secondResult = second.BindOnce<FirstBinder>();

        // Assert
        _ = await Assert.That(firstResult).IsSameReferenceAs(first);
        _ = await Assert.That(secondResult).IsSameReferenceAs(second);
        _ = await Assert.That(first.HasBinding<FirstView>()).IsTrue();
        _ = await Assert.That(second.HasBinding<FirstView>()).IsTrue();
        ValueMember firstField = await Assert.That(first[typeof(FirstView)].GetFields()).HasSingleItem();
        ValueMember secondField = await Assert.That(second[typeof(FirstView)].GetFields()).HasSingleItem();
        _ = await Assert.That(firstField.FieldNumber).IsEqualTo(FieldNumber);
        _ = await Assert.That(secondField.FieldNumber).IsEqualTo(FieldNumber);
    }

    [Test]
    public async Task GivenAFailedBindingWhenItIsRetriedThenOnlyTheSuccessfulCallIsRemembered()
    {
        // Arrange
        const int expectedCalls = 2;
        var model = RuntimeTypeModel.Create();
        RetryingBinder.FailNext();

        // Act
        Exception? exception = Capture(() => _ = model.BindOnce<RetryingBinder>());
        RuntimeTypeModel result = model.BindOnce<RetryingBinder>();
        _ = model.BindOnce<RetryingBinder>();

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(FailureMessage);
        _ = await Assert.That(result).IsSameReferenceAs(model);
        _ = await Assert.That(RetryingBinder.Calls).IsEqualTo(expectedCalls);
        _ = await Assert.That(model.HasBinding<FirstView>()).IsTrue();
        _ = await Assert.That(model[typeof(FirstView)].GetFields()).HasSingleItem();
    }

    [Test]
    public async Task GivenANullModelThenThrowsArgumentNullExceptionWithAMessage()
    {
        // Arrange
        const string expectedMessage = "The runtime type model to bind must be provided.";
        RuntimeTypeModel model = default!;

        // Act
        Exception? exception = Capture(() => _ = model.BindOnce<FirstBinder>());

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

    public readonly record struct FirstView(string Value);

    public readonly record struct SecondView(string Value);

    public sealed class FirstBinder : IBinder
    {
        public static RuntimeTypeModel Bind(RuntimeTypeModel model)
        {
            MetaType contract = model.Add(typeof(FirstView), applyDefaultBehaviour: false);
            _ = contract.Add(FieldNumber, nameof(FirstView.Value));

            return model;
        }
    }

    public sealed class RetryingBinder : IBinder
    {
        private static bool _fail;

        public static int Calls { get; private set; }

        public static RuntimeTypeModel Bind(RuntimeTypeModel model)
        {
            Calls++;

            if (_fail)
            {
                _fail = false;

                throw new InvalidOperationException(FailureMessage);
            }

            return FirstBinder.Bind(model);
        }

        public static void FailNext()
        {
            Calls = 0;
            _fail = true;
        }
    }

    public sealed class SecondBinder : IBinder
    {
        public static RuntimeTypeModel Bind(RuntimeTypeModel model)
        {
            MetaType contract = model.Add(typeof(SecondView), applyDefaultBehaviour: false);
            _ = contract.Add(FieldNumber, nameof(SecondView.Value));

            return model;
        }
    }
}