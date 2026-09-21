# Scribe

`Mu.Communications.Tracing.IScribe` manages the ambient ledger for nested use case execution. Like `AuditScopeManager`, `Scribe` stores its state in the current asynchronous execution context. Instances share that context, while changes in concurrent asynchronous branches remain isolated.

`Next(UseCase)` enters a scope and returns an `IDisposable` lease. Read the active ledger through `IScribe.Ledger`. Without an active scope, the use case's `Identity` becomes both causation and correlation. A nested scope uses its active parent's identity as causation and preserves correlation.

Disposing the lease restores both the previous ledger and the previous use case identity. For A calling B, then calling C after B completes:

| Operation | Active causation | Active correlation |
| --- | --- | --- |
| `Next(A)` | A | A |
| `Next(B)` | A | A |
| Dispose B | A | A |
| `Next(C)` | A | A |

If C is instead entered while B is still active, C's causation is B and its correlation is A.

```csharp
IScribe scribe = new Scribe();
using IDisposable root = scribe.Next(firstUseCase);

using (scribe.Next(secondUseCase))
{
    Ledger child = scribe.Ledger;
    // child.Causation == firstUseCase.Identity
}

using (scribe.Next(thirdUseCase))
{
    Ledger sibling = scribe.Ledger;
    // sibling.Causation == firstUseCase.Identity
}
```

To continue an incoming message chain, call `Set(Ledger)` when no scope is active. It establishes the ledger immediately and returns a disposable lease. The first use case within that scope uses the supplied ledger unchanged; nested use cases use their parent's identity and retain the incoming correlation.

```csharp
using IDisposable incoming = scribe.Set(incomingMessage.Ledger);
using IDisposable execution = scribe.Next(useCase);
Ledger current = scribe.Ledger;
```

Disposing the execution scope restores the incoming ledger. Disposing the incoming scope clears the context, allowing a new root through either `Set` or `Next`. Disposing a root created by `Next` also clears the context.

Use `using` to ensure restoration even when execution throws. Dispose leases in reverse order of creation. Repeated disposal has no effect on the current context. Attempting to dispose a parent before its active child throws `InvalidOperationException` and leaves both scopes intact.

Ardalis guard clauses enforce the following conditions with resource-backed messages:

- `Next(null)` throws `ArgumentNullException` without changing the scope.
- `Set` throws `InvalidOperationException` while any scope is active, including a scope containing an identical or default ledger.
- Reading `Ledger` without an active scope throws `InvalidOperationException`.
- Disposing a parent before its active children throws `InvalidOperationException`.