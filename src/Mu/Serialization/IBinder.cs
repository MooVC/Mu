namespace Mu.Serialization;

using ProtoBuf.Meta;

public interface IBinder
{
    static abstract RuntimeTypeModel Bind(RuntimeTypeModel model);
}