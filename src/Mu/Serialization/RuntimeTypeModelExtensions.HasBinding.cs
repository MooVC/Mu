namespace Mu.Serialization;

using Ardalis.GuardClauses;
using ProtoBuf.Meta;
using static Mu.Serialization.RuntimeTypeModelExtensions_Resources;

public static partial class RuntimeTypeModelExtensions
{
    extension(RuntimeTypeModel model)
    {
        /// <summary>
        /// Determines whether a contract for the specified type has been added to the runtime model.
        /// </summary>
        /// <typeparam name="T">The type whose contract to inspect.</typeparam>
        /// <param name="model">The runtime model to inspect.</param>
        /// <returns>Whether the runtime model contains a contract for the specified type.</returns>
        public bool HasBinding<T>()
        {
            _ = Guard.Against.Null(model, message: HasBindingModelRequired);

            return model.GetTypes()
                .Cast<MetaType>()
                .Any(meta => meta.Type == typeof(T));
        }
    }
}