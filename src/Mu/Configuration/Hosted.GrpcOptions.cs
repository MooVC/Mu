namespace Mu.Configuration;

public partial record Hosted
{
    /// <summary>
    /// Configures a hosted gRPC service.
    /// </summary>
    public sealed record GrpcOptions
    {
        /// <summary>
        /// Gets a value indicating whether detailed exception messages are sent to clients.
        /// </summary>
        public bool EnableDetailedErrors { get; init; }

        /// <summary>
        /// Gets the maximum received message size in bytes, or <see langword="null"/> for no limit.
        /// </summary>
        public int? MaxReceiveMessageSize { get; init; } = 4 * 1024 * 1024;

        /// <summary>
        /// Gets the maximum sent message size in bytes, or <see langword="null"/> for no limit.
        /// </summary>
        public int? MaxSendMessageSize { get; init; }

        /// <summary>
        /// Gets the port on which the gRPC service listens.
        /// </summary>
        public int Port { get; init; } = 50051;
    }
}