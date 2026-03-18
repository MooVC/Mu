namespace Mu.Modelling.NonMutationalTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class WhenValidateIsCalled
{
    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        NonMutational subject = NonMutational.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsTrue();
        _ = await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUnnamedViewThenValidationErrorReturned()
    {
        // Arrange
        NonMutational subject = NonMutational.Undefined.From(NonMutational.Kind.WriteStore);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsFalse();
        _ = await Assert.That(results.Count == 1).IsTrue();
        _ = await Assert.That(results[0].MemberNames).Contains(nameof(NonMutational.View));
    }
}