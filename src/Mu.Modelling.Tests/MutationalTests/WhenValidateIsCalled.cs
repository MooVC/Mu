namespace Mu.Modelling.MutationalTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class WhenValidateIsCalled
{
    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        Mutational subject = Mutational.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsTrue();
        await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUnnamedFactThenValidationErrorReturned()
    {
        // Arrange
        Mutational subject = Mutational.Undefined.OfType(Mutational.Kind.Creational);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsFalse();
        await Assert.That(results.Count == 1).IsTrue();
        await Assert.That(results[0].MemberNames).Contains(nameof(Mutational.Fact));
    }
}