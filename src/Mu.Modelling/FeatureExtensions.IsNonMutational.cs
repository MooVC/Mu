namespace Mu.Modelling
{
    using System;

    public static partial class FeatureExtensions
    {
        public static Feature IsNonMutational(this Feature feature)
        {
            return feature.OfType(Feature.Kinds.NonMutational);
        }

        public static Feature IsNonMutational(this Feature feature, Func<NonMutational, NonMutational> builder)
        {
            return feature
                .IsNonMutational()
                .WithNonMutational(builder);
        }
    }
}