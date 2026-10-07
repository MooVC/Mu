namespace Mu.Persistence.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

public sealed class WriteStoreOptions
{
    public static readonly WriteStoreOptions Default = new();

    public WriteStoreType Type { get; init; } = WriteStoreType.InMemory;
}