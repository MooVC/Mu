namespace MooVC.Testing.Modelling.Account;

using Muify.Domain;

[Unit<Guid>]
public sealed partial record Account(Owner Owner);