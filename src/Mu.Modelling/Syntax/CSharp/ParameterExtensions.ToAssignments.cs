namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Immutable;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.ParameterExtensions_Resources;
    using Parameter = Mu.Modelling.Parameter;

    public static partial class ParameterExtensions
    {
        public static Snippet ToAssignments(this ImmutableArray<Parameter> parameters, Snippet.Options options)
        {
            _ = Guard.Against.Null(options, message: ToAssignmentsOptionsRequired);

            if (parameters.IsDefaultOrEmpty)
            {
                return Snippet.Empty;
            }

            string[] assignments = parameters
                .OrderBy(parameter => parameter.Name)
                .Select(parameter =>
                {
                    var property = parameter.Name.ToSnippet(Variable.Options.Pascal);
                    var variable = parameter.Name.ToSnippet(Variable.Options.Camel);

                    return $"{property} = {variable};";
                })
                .ToArray();

            return Snippet.From(options, assignments);
        }
    }
}