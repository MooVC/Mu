namespace Mu.Modelling.NonMutationalTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class WhenValidateIsCalled
{
    [Test]
    public void GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        NonMutational subject = NonMutational.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        valid.ShouldBeTrue();
        results.ShouldBeEmpty();
    }

    [Test]
    public void GivenUnnamedViewThenValidationErrorReturned()
    {
        // Arrange
        NonMutational subject = NonMutational.Undefined.From(NonMutational.Kind.WriteStore);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        valid.ShouldBeFalse();
        _ = results.ShouldHaveSingleItem();
        results[0].MemberNames.ShouldContain(nameof(NonMutational.View));
    }
}