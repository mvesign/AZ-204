using System;
using System.Threading.Tasks;
using System.Text;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;

namespace EventHub;

public class Producer : IDisposable
{
    private const string EventHubConnectionString = "";
    private const string EventHubName = "";

    private readonly EventHubProducerClient _producerClient;

    public Producer()
    {
        _producerClient = new EventHubProducerClient(EventHubConnectionString, EventHubName);     
    }

    public async Task SendEventsToEventHub(int numberOfEvents)
    {
        for (var count = 0; count < numberOfEvents; count++)
        {
            try
            {
                var message = $"Event #{count}";
                Console.WriteLine($"Sending event: {message}");
                await _producerClient.SendAsync(
                [
                    new EventData(Encoding.UTF8.GetBytes(message))
                ]);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }

            await Task.Delay(10);
        }

        Console.WriteLine($"{numberOfEvents} events sent.");
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

        _producerClient?.CloseAsync().Wait();
    }
}