using Azure.Messaging.ServiceBus;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureMessageQueue;

public class ServiceBusQueueClient : IDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusQueueClient(string connectionString, string queueName)
    {
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task Send(int numberOfMessages)
    {
        // create a batch 
        using var messageBatch = await _sender.CreateMessageBatchAsync();

        Enumerable.Range(0, numberOfMessages).ToList().ForEach(x =>
        {
            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {x}")))
                throw new Exception($"Exception on message {x} has occurred.");
        });
        
        await _sender.SendMessagesAsync(messageBatch);
        Console.WriteLine($"A batch of three messages has been published to the queue.");
        Console.WriteLine("Press any key to continue");
        _ = Console.ReadKey();
    }

    public void Dispose()
    {
        Dispose(true).Wait();
        GC.SuppressFinalize(this);
    }

    protected virtual async Task Dispose(bool disposing)
    {
        if (_client != null)
            await _client.DisposeAsync();
        if (_sender != null)
            await _sender.DisposeAsync();
    }
}