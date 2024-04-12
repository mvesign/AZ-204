using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace AzureMessageQueue;

public class ServiceBusQueueProcessor : IDisposable
{
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusClient _client;

    public ServiceBusQueueProcessor(string connectionString, string queueName)
    {
        _client = new ServiceBusClient(connectionString);
        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
    }

    public async Task Process()
    {
        await _processor.StartProcessingAsync();

        Console.WriteLine("Wait for a minute and then press any key to end the processing");
        _ = Console.ReadKey();

        Console.WriteLine("Stopping the receiver...");
        await _processor.StopProcessingAsync();
        Console.WriteLine("Stopped receiving messages");
    }

    private static Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");

        return args.CompleteMessageAsync(args.Message);
    }

    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
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
        if (_processor != null)
            await _processor.DisposeAsync();
    }
}