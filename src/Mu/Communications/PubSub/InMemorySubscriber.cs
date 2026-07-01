namespace Mu.Communications.PubSub;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mu.Communications.Mediation;
using Mu.Communications.Messaging;

/// <summary>
/// Represents an in-memory subscriber that reads events from a channel and raises the Received event for each event received.
/// </summary>
/// <param name="channel">The channel from which to read events.</param>
/// <remarks>This is not considered a production-ready implementation and should be used for testing only.</remarks>
public sealed partial class InMemorySubscriber(ChannelReader<Event> channel, ILogger<InMemorySubscriber> logger)
    : BackgroundService,
      ISubscriber
{
    /// <inheritdoc/>
    public event ISubscriber.EventReceivedHandler? Received;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (Event @event in channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                LogExecutionRequested(logger, @event);

                await (Received?.Invoke(this, @event, stoppingToken) ?? Task.CompletedTask);

                LogExecutionSucceeded(logger, @event);
            }
            catch (Exception cause)
            {
                LogExecutionFailed(logger, @event, cause);
            }
        }
    }

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to handle receipt of `{Event}`")]
    private static partial void LogExecutionFailed(ILogger logger, Event @event, Exception exception);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Attempting to notify subscribers of `{Event}`")]
    private static partial void LogExecutionRequested(ILogger logger, Event @event);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Successfully notified subscribers of `{Event}`")]
    private static partial void LogExecutionSucceeded(ILogger logger, Event @event);
}