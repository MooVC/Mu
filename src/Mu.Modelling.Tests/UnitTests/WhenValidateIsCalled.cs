namespace Mu.Modelling.UnitTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MooVC.Syntax.Elements;

public sealed class WhenValidateIsCalled
{
    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        Unit subject = Unit.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsTrue();
        _ = await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUnnamedNameThenValidationErrorReturned()
    {
        // Arrange
        Unit subject = Unit.Undefined
            .Featuring(ModellingTestData.CreateFeature())
            .Named(Name.Unnamed);

        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsFalse();
        _ = await Assert.That(results.Count == 1).IsTrue();
        _ = await Assert.That(results[0].MemberNames).Contains(nameof(Unit.Name));
    }
}