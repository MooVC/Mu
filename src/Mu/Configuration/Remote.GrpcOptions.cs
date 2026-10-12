namespace Mu.Configuration;

public partial record Remote
{
    /// <summary>
    /// Configures a client consuming a remote gRPC service.
    /// </summary>
    public sealed record GrpcOptions
    {
        /// <summary>
        /// Gets the address of the remote gRPC service.
        /// </summary>
        public Uri Address { get; init; } = new("http://localhost:50051");

        /// <summary>
        /// Gets the maximum received message size in bytes, or <see langword="null"/> for no limit.
        /// </summary>
        public int? MaxReceiveMessageSize { get; init; } = 4 * 1024 * 1024;

        /// <summary>
        /// Gets the maximum sent message size in bytes, or <see langword="null"/> for no limit.
        /// </summary>
        public int? MaxSendMessageSize { get; init; }

        /// <summary>
        /// Gets the time allowed for a remote call. Defaults to thirty seconds.
        /// </summary>
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    }
}