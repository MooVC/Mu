namespace Mu.Communications.Ipc.Grpc.Composition;

using Ardalis.GuardClauses;
using global::Grpc.Net.Client;
using Mu.Communications.Tracing;
using Mu.Configuration;
using ProtoBuf.Grpc.Client;
using SimpleInjector;
using static Mu.Communications.Ipc.Grpc.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        /// <summary>
        /// Registers a scoped client backed by its own reusable gRPC channel.
        /// </summary>
        /// <typeparam name="TContract">The remote gRPC contract.</typeparam>
        /// <typeparam name="TClient">The application client contract.</typeparam>
        /// <param name="options">The remote gRPC settings.</param>
        /// <param name="factory">Creates the application client using its proxy, scribe, and timeout.</param>
        /// <returns>The container.</returns>
        public Container RegisterGrpcClient<TContract, TClient>(Remote.GrpcOptions options, Func<TContract, IScribe, TimeSpan, TClient> factory)
            where TContract : class
            where TClient : class
        {
            _ = Guard.Against.Null(container, message: RegisterGrpcClientContainerRequired);
            _ = Guard.Against.Null(options, message: RegisterGrpcClientOptionsRequired);
            _ = Guard.Against.Null(factory, message: RegisterGrpcClientFactoryRequired);

            var channel = GrpcChannel.ForAddress(options.Address, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = options.MaxReceiveMessageSize,
                MaxSendMessageSize = options.MaxSendMessageSize,
            });

            try
            {
                container.ContainerScope.RegisterForDisposal(channel);
                TContract proxy = channel.CreateGrpcService<TContract>();

                container.RegisterInstance(proxy);
                container.Register(() => factory(proxy, container.GetInstance<IScribe>(), options.Timeout), Lifestyle.Scoped);

                return container;
            }
            catch
            {
                channel.Dispose();

                throw;
            }
        }
    }
}