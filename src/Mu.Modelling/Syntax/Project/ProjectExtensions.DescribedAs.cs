namespace Mu.Modelling.Components.Syntax.Project
{
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.Project;

    public static partial class ProjectExtensions
    {
        public static Project DescribedAs(this Project project, Description description)
        {
            _ = Guard.Against.Null(project, message: DescribedAsProjectRequired);
            _ = Guard.Against.Null(description, message: DescribedAsDescriptionRequired);

            return project.ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: subject => subject.WithPropertyGroups(group => group
                    .WithProperty(nameof(Description), description)));
        }
    }
}