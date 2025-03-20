namespace Mu.Sample.Open;

using System.Threading;
using Mu.Persistence;
using Mu.Sample.Account;
using Mu.Services;

public sealed class Service(IRoot<Account, Open> root, IWriteStore<Account, Guid> store)
    : IService<Open, Guid>
{
    public Task<Result<Guid>> Execute(Open open, CancellationToken cancellationToken)
    {
        var account = new Account();
        var identity = Guid.CreateVersion7();

        Result<Account> opened = root.Apply(account, open);

        return opened.Select(async opened =>
        {
            await store
                .Save(account, identity, cancellationToken)
                .ConfigureAwait(false);

            return identity;
        });
    }
}