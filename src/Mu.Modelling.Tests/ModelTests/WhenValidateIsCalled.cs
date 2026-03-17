namespace Mu.Modelling.ModelTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class WhenValidateIsCalled
{
    private const string CompanyNameValue = "Company";

    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        Model subject = Model.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsTrue();
        await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUnnamedNameThenValidationErrorReturned()
    {
        // Arrange
        Model subject = Model.Undefined.For(CompanyNameValue);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsFalse();
        await Assert.That(results.Count == 1).IsTrue();
        await Assert.That(results[0].MemberNames.Contains(nameof(Model.Name))).IsTrue();
    }
}