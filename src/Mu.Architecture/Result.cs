namespace Mu.Architecture;

using System.Text.Json.Serialization;

public sealed record Result<T>
{
    [JsonConstructor]
    internal Result(bool isSuccess, T value)
    {
        IsSuccess = isSuccess;
        Value = value;
    }

    public bool IsSuccess { get; }

    public T Value { get; }
}