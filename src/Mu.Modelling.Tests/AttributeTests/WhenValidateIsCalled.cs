namespace Mu.Modelling.AttributeTests;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MooVC.Syntax.Elements;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenValidateIsCalled
{
    [Test]
    public async Task GivenUndefinedThenValidationIsSkipped()
    {
        // Arrange
        ModellingAttribute subject = ModellingAttribute.Undefined;
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsTrue();
        _ = await Assert.That(results.Count == 0).IsTrue();
    }

    [Test]
    public async Task GivenMultiLineDefaultThenValidationErrorReturned()
    {
        // Arrange
        Snippet defaultValue = Snippet.From($"Alpha{Environment.NewLine}Beta");
        ModellingAttribute subject = ModellingTestData.CreateAttribute(defaultValue: defaultValue);
        var context = new ValidationContext(subject);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(subject, context, results, validateAllProperties: true);

        // Assert
        _ = await Assert.That(valid).IsFalse();
        _ = await Assert.That(results.Count == 1).IsTrue();
        _ = await Assert.That(results[0].MemberNames).Contains(nameof(ModellingAttribute.Default));
    }
}