namespace Mu;

/// <summary>
/// Represents an inclusive range between two comparable values.
/// </summary>
/// <typeparam name="T">The comparable value type bounded by the range.</typeparam>
public readonly record struct Range<T>
    where T : IComparable<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Range{T}"/> record.
    /// </summary>
    /// <param name="from">The inclusive lower bound for the range.</param>
    /// <param name="to">The inclusive upper bound for the range.</param>
    public Range(T from, T to)
    {
        if (from.CompareTo(to) > 0)
        {
            throw new ArgumentException($"The {nameof(From)} value of `{from}` must be lower than the To value {nameof(To)} `{to}`.", nameof(to));
        }

        From = from;
        To = to;
    }

    /// <summary>
    /// Gets the inclusive lower bound for the range.
    /// </summary>
    public T From { get; }

    /// <summary>
    /// Gets the inclusive upper bound for the range.
    /// </summary>
    public T To { get; }

    /// <summary>
    /// Converts a tuple to a <see cref="Range{T}"/>.
    /// </summary>
    /// <param name="range">The tuple containing the lower and upper bounds.</param>
    public static implicit operator Range<T>((T From, T To) range)
    {
        return new Range<T>(range.From, range.To);
    }
}