namespace Mu.Modelling.AreaTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class WhenValidateIsCalled
{
    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        Area subject = Area.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsTrue();
        await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUndefinedUnitThenValidationErrorReturned()
    {
        // Arrange
        Area subject = Area.Undefined.ResponsibleFor(Unit.Undefined);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsFalse();
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].MemberNames.Contains(nameof(Area.Units))).IsTrue();
        await Assert.That(results[1].MemberNames.Contains(nameof(Unit.Name))).IsTrue();
    }
}