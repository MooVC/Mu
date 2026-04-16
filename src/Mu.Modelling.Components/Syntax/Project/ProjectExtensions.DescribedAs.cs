namespace Mu.Modelling.Components.Syntax.Project;

using MooVC;
using MooVC.Syntax.Project;

internal static partial class ProjectExtensions
{
    public static Project DescribedAs(this Project project, Description description)
    {
        return project.ForkOn(
            _ => description.IsUndescribed,
            @true: _ => _,
            @false: project => project.WithPropertyGroups(group => group
                .WithProperty(nameof(Description), description)));
    }
}