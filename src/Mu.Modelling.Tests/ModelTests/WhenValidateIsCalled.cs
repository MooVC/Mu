namespace Mu.Modelling.ModelTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MooVC.Syntax;

public sealed class WhenValidateIsCalled
{
    private const string AreaNameValue = "Area";
    private const string CompanyNameValue = "Company";
    private const string InvalidScopeValue = "Extra.Invalid-Node";
    private const string InvalidSegmentValue = "Invalid-Node";
    private const string ModelNameValue = "Model";

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
        _ = await Assert.That(valid).IsTrue();
        _ = await Assert.That(results).IsEmpty();
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
        _ = await Assert.That(valid).IsFalse();
        _ = await Assert.That(results).HasAtLeast(1);
        _ = await Assert.That(results.SelectMany(result => result.MemberNames)).Contains(nameof(Model.Name));
    }

    [Test]
    [Arguments("")]
    [Arguments("Extra")]
    [Arguments("Extra.One")]
    public async Task GivenValidScopeThenValidationSucceeds(string scope)
    {
        // Arrange
        Model subject = Model.Undefined
            .Defines(area => area.Named(AreaNameValue))
            .For(CompanyNameValue)
            .Named(ModelNameValue)
            .ScopedWithin(scope);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsTrue();
        _ = await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task GivenInvalidScopeThenValidationErrorReturned()
    {
        // Arrange
        Model subject = Model.Undefined
            .Defines(area => area.Named(AreaNameValue))
            .For(CompanyNameValue)
            .Named(ModelNameValue)
            .ScopedWithin(InvalidScopeValue);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsFalse();
        ValidationResult result = await Assert.That(results).HasSingleItem();
        _ = await Assert.That(result.MemberNames).IsEquivalentTo([nameof(Name)]);
        _ = await Assert.That(result.ErrorMessage).Contains(InvalidSegmentValue);
    }
}