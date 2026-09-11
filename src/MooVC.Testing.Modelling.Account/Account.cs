namespace MooVC.Testing.Modelling.Account;

using Muify.Domain;

[Unit<Guid>]
public sealed partial record Account
{
    public Owner Owner { get; init; } = Owner.Unspecified;
}