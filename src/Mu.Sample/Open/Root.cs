namespace Mu.Sample.Open;

using Mu.Modelling.State;
using Mu.Sample.Account;
using Mu.Services;

public sealed class Root
    : IRoot<Account, Open>
{
    public Account Apply(Account account, Open open)
    {
        var opened = new Opened(open.Owner);

        return account.Propose(opened) with
        {
            Owner = open.Owner,
        };
    }
}