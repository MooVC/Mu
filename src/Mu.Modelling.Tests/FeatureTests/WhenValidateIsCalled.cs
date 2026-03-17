namespace Mu.Modelling.FeatureTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MooVC.Syntax.Elements;

public sealed class WhenValidateIsCalled
{
    private const string FeatureNameValue = "Feature";

    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        Feature subject = Feature.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsTrue();
        await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenUndefinedMutationalThenValidationErrorReturned()
    {
        // Arrange
        Feature subject = Feature.Undefined.Named(FeatureNameValue);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        await Assert.That(valid).IsFalse();
        await Assert.That(results.Count == 1).IsTrue();
        await Assert.That(results[0].MemberNames.Contains(nameof(Feature.Mutational))).IsTrue();
    }
}