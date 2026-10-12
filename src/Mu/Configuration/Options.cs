namespace Mu.Configuration;

public record Options
{
    public ModeType Mode { get; init; } = ModeType.Hosted;

    public Hosted Hosted { get; init; } = new();

    public Remote Remote { get; init; } = new();
}