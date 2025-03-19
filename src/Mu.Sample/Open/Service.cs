namespace Mu.Sample.Open;

using Mu.Persistence;
using Mu.Sample.Account;
using Mu.Services;

public sealed class Service(IRoot<Account, Open> root, IWriteStore<Account, Guid> store)
    : IService<Open, Guid>
{
    public async Task<Guid> Execute(Open open, CancellationToken cancellationToken)
    {
        var account = new Account();
        var identity = Guid.CreateVersion7();

        account = root.Apply(account, open);

        await store
            .Save(account, identity, cancellationToken)
            .ConfigureAwait(false);

        return identity;
    }
}