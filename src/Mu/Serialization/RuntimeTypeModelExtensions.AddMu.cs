namespace Mu.Serialization;

using System.Runtime.CompilerServices;
using ProtoBuf.Meta;

/// <summary>
/// Provides Mu contract configuration for protobuf-net runtime models.
/// </summary>
public static partial class RuntimeTypeModelExtensions
{
    private static readonly ConditionalWeakTable<RuntimeTypeModel, object> _models = new();
    private static readonly Lock _sync = new();

    /// <summary>
    /// Adds Mu's shared protobuf-net contract configuration to the runtime model.
    /// </summary>
    /// <param name="model">The runtime model to configure.</param>
    /// <returns>The configured runtime model.</returns>
    public static RuntimeTypeModel AddMu(this RuntimeTypeModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        lock (_sync)
        {
            if (!_models.TryGetValue(model, out _))
            {
                MetaType type = model.Add(typeof(DateTimeOffset), false);

                type.SetSurrogate(typeof(DateTimeOffsetSurrogate));

                _models.Add(model, new object());
            }
        }

        return model;
    }
}