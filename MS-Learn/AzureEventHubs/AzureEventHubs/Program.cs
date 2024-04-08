using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureEventHubs;

public static class Program
{
    private const string EventHubConnectionString = "[EVENT-HUB-CONNECTIONSTRING]";
    private const string EventHubName = "[EVENT-HUB-NAME]";
    private const string StorageConnectionString = "[STORAGE-ACCOUNT-CONNECTIONSTRING]";
    private const string BlobContainerName = "[BLOB-STORAGE-ACCOUNT-NAME]";

    private static async Task Main(string[] args)
    {
        await using var producerClient = new EventHubProducerClient(
            EventHubConnectionString,
            EventHubName);
        await producerClient.GetPartitionIds();
        await producerClient.SendEventViaAutomaticRouting();

        await using var consumerClient = new EventHubConsumerClient(
            EventHubConsumerClient.DefaultConsumerGroupName,
            EventHubConnectionString,
            EventHubName);
        await consumerClient.ReadAllEvents();
        await consumerClient.ReadEventFromPartition();

        var processorClient = new EventProcessorClient(
            new BlobContainerClient(StorageConnectionString, BlobContainerName),
            EventHubConsumerClient.DefaultConsumerGroupName,
            EventHubConnectionString,
            EventHubName);
        await processorClient.ReadEventsToStorageAccount();
    }

    /// <summary>
    /// Get all available partitions.
    /// </summary>
    private static async Task GetPartitionIds(this EventHubProducerClient producerClient)
    {
        var partitionIds = await producerClient.GetPartitionIdsAsync();
        Console.WriteLine($"Available partitions: {string.Join(", ", partitionIds)}");
    }
    
    /// <summary>
    /// Send an event, without specifying a partition to use the automatic routing of Event Hubs.
    /// </summary>
    private static async Task SendEventViaAutomaticRouting(this EventHubProducerClient producerClient)
    {
        using var eventBatch = await producerClient.CreateBatchAsync();
        _ = eventBatch.TryAdd(new EventData(new BinaryData("First")));
        _ = eventBatch.TryAdd(new EventData(new BinaryData("Second")));

        await producerClient.SendAsync(eventBatch);
        Console.WriteLine("Send event batch of multiple events, via automatic routing");
    }

    /// <summary>
    /// Read all events from the EvenHubConsumerClient.
    /// </summary>
    /// <remarks>
    /// This is not the recommened way. Using the EventProcessorClient is.
    /// </remarks>
    private static async Task ReadAllEvents(this EventHubConsumerClient consumerClient)
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(TimeSpan.FromSeconds(45));

        await foreach (var receivedEvent in consumerClient.ReadEventsAsync(cancellationSource.Token))
        {
            // At this point, the loop will wait for events to be available in the Event Hub.  When an event
            // is available, the loop will iterate with the event that was received.  Because we did not
            // specify a maximum wait time, the loop will wait forever unless cancellation is requested using
            // the cancellation token.
            Console.WriteLine($"Read event: {receivedEvent}");
        }
    }

    /// <summary>
    /// Read all events from a specific partitons, from the EvenHubConsumerClient.
    /// </summary>
    /// <remarks>
    /// This is not the recommened way. Using the EventProcessorClient is.
    /// </remarks>
    private static async Task ReadEventFromPartition(this EventHubConsumerClient consumerClient)
    {
        var partitionIds = await consumerClient.GetPartitionIdsAsync();
        if (partitionIds.Length == 0)
        {
            Console.WriteLine("No partitions available. stop reading events.");
            return;
        }

        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(TimeSpan.FromSeconds(45));

        // Read from the first known partition, for demo purposes.
        // Also read from the first occurred event, for demo purposes.
        await foreach (var receivedEvent in consumerClient.ReadEventsFromPartitionAsync(
            partitionIds[0], EventPosition.Earliest, cancellationSource.Token))
        {
            // At this point, the loop will wait for events to be available in the partition.  When an event
            // is available, the loop will iterate with the event that was received.  Because we did not
            // specify a maximum wait time, the loop will wait forever unless cancellation is requested using
            // the cancellation token.
            Console.WriteLine($"Read event: {receivedEvent}");
        }
    }

    /// <summary>
    /// Read events from Event Hubs and write to a storage account, via EventProcessorClient.
    /// </summary>
    private static async Task ReadEventsToStorageAccount(this EventProcessorClient processorClient)
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(TimeSpan.FromSeconds(45));

        processorClient.ProcessEventAsync += processEventHandler;
        processorClient.ProcessErrorAsync += processErrorHandler;

        await processorClient.StartProcessingAsync();

        try
        {
            // The processor performs its work in the background.
            // Block until cancellation to allow processing to take place.
            await Task.Delay(Timeout.Infinite, cancellationSource.Token);
        }
        catch (TaskCanceledException)
        {
            // This is expected when the delay is canceled.
        }

        try
        {
            await processorClient.StopProcessingAsync();
        }
        finally
        {
            // To prevent leaks, the handlers should be removed when processing is complete.
            processorClient.ProcessEventAsync -= processEventHandler;
            processorClient.ProcessErrorAsync -= processErrorHandler;
        }

        Task processEventHandler(ProcessEventArgs eventArgs)
        {
            Console.WriteLine($"Event processed: {eventArgs}");
            return Task.CompletedTask;
        }

        Task processErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Console.WriteLine($"Error occurred during event process: {eventArgs.Exception}");
            return Task.CompletedTask;
        }
    }
}