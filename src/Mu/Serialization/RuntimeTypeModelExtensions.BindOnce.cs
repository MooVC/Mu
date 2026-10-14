namespace Mu.Serialization;

using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using ProtoBuf.Meta;
using static Mu.Serialization.RuntimeTypeModelExtensions_Resources;

public static partial class RuntimeTypeModelExtensions
{
    private static readonly ConditionalWeakTable<RuntimeTypeModel, HashSet<Type>> _binders = new();
    private static readonly Lock _bindingSync = new();

    extension(RuntimeTypeModel model)
    {
        /// <summary>
        /// Applies a binder once to this runtime model, recording it only after binding succeeds.
        /// </summary>
        /// <typeparam name="TBinder">The binder to apply.</typeparam>
        /// <param name="model">The runtime model to configure.</param>
        /// <returns>The configured runtime model.</returns>
        public RuntimeTypeModel BindOnce<TBinder>()
            where TBinder : IBinder
        {
            _ = Guard.Against.Null(model, message: BindOnceModelRequired);

            lock (_bindingSync)
            {
                HashSet<Type> binders = _binders.GetOrCreateValue(model);

                if (!binders.Contains(typeof(TBinder)))
                {
                    _ = TBinder.Bind(model);
                    _ = binders.Add(typeof(TBinder));
                }
            }

            return model;
        }
    }
}