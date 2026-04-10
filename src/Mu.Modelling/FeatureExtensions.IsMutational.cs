namespace Mu.Modelling;

public static partial class FeatureExtensions
{
    public static Feature IsMutational(this Feature feature, Func<Mutational, Mutational> builder)
    {
        return feature
            .OfType(Feature.Kinds.Mutational)
            .WithMutational(builder);
    }
}