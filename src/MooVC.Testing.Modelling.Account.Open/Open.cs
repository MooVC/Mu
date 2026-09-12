namespace MooVC.Testing.Modelling.Account.Open;

using Muify.Service;

[Creational<Opened>]
public sealed partial record Open
{
    public Owner Owner { get; init; } = Owner.Unspecified;
}