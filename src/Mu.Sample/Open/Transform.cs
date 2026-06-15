namespace Mu.Sample.Open;

using Mu.Modelling.Services;
using Mu.Sample.Account;

internal sealed class Transform
    : ITransform<Account, Opened>
{
    public Account Apply(Account account, Opened opened)
    {
        return account with
        {
            Owner = opened.Owner,
        };
    }
}