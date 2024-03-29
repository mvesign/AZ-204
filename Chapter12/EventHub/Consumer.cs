
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventHub;

public class Consumer : IDisposable
{
    private const string EventHubConnectionString = "";
    private const string EventHubName = "";
    private const string StorageConnectionString  = "";
    private const string StorageName = "";

    private readonly BlobContainerClient _storageClient;
    private readonly EventProcessorClient _processorClient;

    public Consumer()
    {
        _storageClient = new BlobContainerClient(StorageConnectionString, StorageName);
        _processorClient = new EventProcessorClient(
            _storageClient, EventHubConsumerClient.DefaultConsumerGroupName, EventHubConnectionString, EventHubName
        );
        _processorClient.ProcessEventAsync += ProcessEventHandler;
        _processorClient.ProcessErrorAsync += ProcessErrorHandler;
    }

    public Task StartConsuming()
        => _processorClient.StartProcessingAsync();

    public Task StopConsuming()
        => _processorClient.StopProcessingAsync();

    private Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        // Write the body of the event to the console window
        Console.WriteLine("Received event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
        return Task.CompletedTask;
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
    {
        // Write details about the error to the console window
        Console.WriteLine($"Partition '{eventArgs.PartitionId}': an unhandled exception was encountered.");
        Console.WriteLine(eventArgs.Exception.Message);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        if (_processorClient.IsRunning)
            _processorClient.StopProcessing();
    }
}