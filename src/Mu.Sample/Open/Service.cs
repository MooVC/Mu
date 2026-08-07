namespace Mu.Sample.Open;

using System.Threading;
using Mu.Modelling.Services;
using Mu.Persistence;
using Mu.Sample.Account;

public sealed class Service(IRoot<Account, Open> root, IWriteStore<Account, Guid> store)
    : IService<Open, Open.Result>
{
    public Task<Result<Open.Result>> Execute(Open open, CancellationToken cancellationToken)
    {
        var account = new Account();
        var identity = Guid.CreateVersion7();

        return root
            .Apply(account, open, cancellationToken)
            .Then(opened => store.Save(opened, identity, cancellationToken))
            .Select(_ => new Open.Result(identity));
    }
}