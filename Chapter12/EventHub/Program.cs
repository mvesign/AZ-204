using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub;

public static class Program
{
    private static async Task Main(string[] args)
    {
        using var producer = new Producer();
        await producer.SendEventsToEventHub(10);

        Thread.Sleep(TimeSpan.FromSeconds(10));

        using var consumer = new Consumer();
        await consumer.StartConsuming();
        Thread.Sleep(TimeSpan.FromSeconds(30));
        await consumer.StopConsuming();
    }
}