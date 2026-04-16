namespace Mu.Modelling
{
    public static partial class MutationalExtensions
    {
        public static Mutational IsCreational(this Mutational mutational)
        {
            return mutational.OfType(Mutational.Kinds.Creational);
        }
    }
}