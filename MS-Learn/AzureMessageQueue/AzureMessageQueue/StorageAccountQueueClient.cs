using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureMessageQueue;

public class StorageAccountQueueClient(string connectionString, string queueName)
{
    private readonly QueueClient _client = new(connectionString, queueName);

    public Task Setup()
        => _client.CreateIfNotExistsAsync();

    public async Task SendMessages(int numberOfMessage)
    {
        if (await _client.ExistsAsync())
            Enumerable.Range(0, numberOfMessage).ToList()
                .ForEach(async (x) =>
                    _ = await _client.SendMessageAsync($"Message {x}")
                );
    }

    public async Task PeekMessages(int numberOfMessages)
    {
        if (await _client.ExistsAsync())
        { 
            // Peek at the next message
            PeekedMessage[] peekedMessage = await _client.PeekMessagesAsync(numberOfMessages);
            peekedMessage.ToList()
                .ForEach(message =>
                    Console.WriteLine($"{message.MessageId}: {message.MessageText}")
                );
        }
    }

    public async Task ReadQueueLength(int numberOfMessages)
    {
        if (await _client.ExistsAsync())
        {
            QueueProperties properties = await _client.GetPropertiesAsync();

            // Retrieve the cached approximate message count. Which isn't less, but can be higher than the actual queue size.
            var cachedMessagesCount = properties.ApproximateMessagesCount;
            Console.WriteLine($"Expected number of messages in queue: {numberOfMessages}");
            Console.WriteLine($"Number of messages in queue: {cachedMessagesCount}");
        }
    }

    public async Task UpdateMessage()
    {
        if (await _client.ExistsAsync())
        {
            QueueMessage[] message = await _client.ReceiveMessagesAsync();
            _ = await _client.UpdateMessageAsync(
                message[0].MessageId, 
                message[0].PopReceipt, 
                "Updated contents",
                TimeSpan.FromSeconds(60.0)  // Make it invisible for another 60 seconds
            );
        }
    }

    public async Task ReadMessages(int numberOfMessages)
    {
        if (await _client.ExistsAsync())
        {
            QueueMessage[] retrievedMessage = await _client.ReceiveMessagesAsync(numberOfMessages);

            // Process (i.e. print) the message in less than 30 seconds, or else if will be visible for other consumers.
            Console.WriteLine($"Dequeued message: '{retrievedMessage[0].Body}'");

            _ = await _client.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
        }
    }

    public async Task DeleteQueue()
    {
        if (await _client.ExistsAsync())
            _ = await _client.DeleteAsync();
    }
}