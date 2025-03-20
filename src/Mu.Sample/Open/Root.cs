namespace Mu.Sample.Open;

using Mu.Modelling.Integrity;
using Mu.Modelling.State;
using Mu.Sample.Account;
using Mu.Services;

public sealed class Root(IEnumerable<IInvariant<Account, Open>> invariants)
    : IRoot<Account, Open>
{
    public Result<Account> Apply(Account account, Open open)
    {
        var opened = new Opened(open.Owner);

        return account.Propose(opened) with
        {
            Owner = open.Owner,
        };
    }
}