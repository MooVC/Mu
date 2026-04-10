namespace Mu.Modelling;

public static partial class MutationalExtensions
{
    public static NonMutational FromWriteStore(this NonMutational nonMutational)
    {
        return nonMutational.From(NonMutational.Kinds.WriteStore);
    }
}