using System.Linq;
using System.Threading.Tasks;

namespace AzureMessageQueue;

public static class Program
{
    private const string ServiceBusQueueConnectionString = "[CONNECTIONSTRING]";
    private const string ServiceBusQueueName = "mjoy-sbq";
    private const string StorageAccountQueueConnectionString = "[CONNECTIONSTRING]";
    private const string StorageAccountQueueName = "mjoy-qs";
    private const int NumberOfMessagesPerQueue = 3;

    private static async Task Main(string[] args)
    {
        var queueClient = new ServiceBusQueueClient(ServiceBusQueueConnectionString, ServiceBusQueueName);
        await queueClient.Send(NumberOfMessagesPerQueue);

        var queueProcessor = new ServiceBusQueueProcessor(ServiceBusQueueConnectionString, ServiceBusQueueName);
        await queueProcessor.Process();

        if (args.Contains("demo-queue-storage"))
        {
            var queueStorageClient = new StorageAccountQueueClient(StorageAccountQueueConnectionString, StorageAccountQueueName);
            await queueStorageClient.Setup();
            await queueStorageClient.SendMessages(NumberOfMessagesPerQueue);
            await queueStorageClient.PeekMessages(NumberOfMessagesPerQueue);
            await queueStorageClient.ReadQueueLength(NumberOfMessagesPerQueue);
            await queueStorageClient.ReadMessages(NumberOfMessagesPerQueue);
            await queueStorageClient.DeleteQueue();
        }
    }
}