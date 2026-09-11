namespace MooVC.Testing.Modelling.Account;

public sealed partial record Owner(string Name)
{
    public static readonly Owner Unspecified = new();

    private Owner()
        : this(string.Empty)
    {
    }
}