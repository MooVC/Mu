namespace Mu.Modelling.Behavior;

public interface IConvertFrom<out TSelf, in TSource>
    where TSelf : IConvertFrom<TSelf, TSource>
{
    static abstract implicit operator TSelf(TSource subject);
}