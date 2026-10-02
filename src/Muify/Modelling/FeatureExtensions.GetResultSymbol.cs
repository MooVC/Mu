namespace Muify.Modelling
{
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Feature = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Features.Feature;

    internal static partial class FeatureExtensions
    {
        public static Symbol GetResultSymbol(this Feature feature)
        {
            if (feature.Value.Results.Length > 0 || !feature.Value.Type.IsMutational)
            {
                return (Name: $"{feature.Value.Name}.Result", Qualifier: feature.Namespace);
            }

            return feature.Value.Mutational.Type.IsCreational
                ? feature.Features.Unit.Value.Identity.GetSymbol(feature.Features.Unit.Namespace)
                : (Symbol)(Name: "Revision", Qualifier: "Mu.Modelling.State");
        }
    }
}