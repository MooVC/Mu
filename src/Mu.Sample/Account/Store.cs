namespace Mu.Sample.Account;

using System.Collections.Concurrent;
using Mu.Persistence;

internal sealed class Store
    : IWriteStore<Account, Guid>
{
    private readonly ConcurrentDictionary<Guid, Account> _accounts = [];

    public Task<Account?> Get(Guid identity, ulong revision, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = _accounts.TryGetValue(identity, out Account? account);
        return Task.FromResult(account);
    }

    public Task Save(Account aggregate, Guid identity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        cancellationToken.ThrowIfCancellationRequested();

        _accounts[identity] = aggregate;

        return Task.CompletedTask;
    }
}