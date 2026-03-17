namespace Mu.Modelling.ViewTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MooVC.Syntax.Elements;

public sealed class WhenValidateIsCalled
{
    private const string ViewNameValue = "View";

    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        View subject = View.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsTrue();
        await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUnqualifiedFactThenValidationErrorReturned()
    {
        // Arrange
        View subject = View.Undefined
            .Named(ViewNameValue)
            .RenderedOn(Qualifier.Unqualified);

        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsFalse();
        await Assert.That(results.Count == 1).IsTrue();
        await Assert.That(results[0].MemberNames.Contains(nameof(View.Facts))).IsTrue();
    }
}