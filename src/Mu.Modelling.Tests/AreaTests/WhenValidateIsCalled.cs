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
        _ = await Assert.That(valid).IsTrue();
        _ = await Assert.That(results).IsEmpty();
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
        _ = await Assert.That(valid).IsFalse();
        _ = await Assert.That(results.Count).IsEqualTo(2);
        _ = await Assert.That(results[0].MemberNames).Contains(nameof(Unit.Name));
        _ = await Assert.That(results[1].MemberNames).Contains(nameof(Area.Units));
    }
}