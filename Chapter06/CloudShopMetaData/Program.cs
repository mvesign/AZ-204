using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudShopMetaData;

public static class Program
{
    public const string ConnectionString = "";
    public const string ContainerName = "cloudshopmeta";

    private static async Task Main(string[] args)
    {
        var blobServiceClient = new BlobServiceClient(ConnectionString);

        Console.WriteLine("Part 1: Creating customers");

        var blobContainerClient = await blobServiceClient.BuildBlobContainer();

        var orders = new Order[]
        {
            new(1, "Woodrow"),
            new(2, "ContosoLLC"),
            new(3, "Woodrow")
        };
        foreach(var order in orders)
            await blobContainerClient.UploadOrder(order);

        Console.WriteLine("Part 2: Search for customers");

        var list = await blobServiceClient.FindOrder("Woodrow");
        foreach(var customer in list)
            Console.WriteLine($"Found id = {customer.Id}");

        Console.WriteLine("Part 3: Pull meta data");

        blobServiceClient.GetContainerMetadata();
    }

    public static async Task<BlobContainerClient> BuildBlobContainer(this BlobServiceClient blobServiceClient)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        _ = await blobContainerClient.CreateIfNotExistsAsync();
        _ = await blobContainerClient.SetMetadataAsync(new Dictionary<string, string>()
        {
            { "Creator" , "TheCloudShopsMetaData"  },
            { "Department", ContainerName  }
        });

        return blobContainerClient;
    }

    public static async Task UploadOrder(this BlobContainerClient container, Order order)
    {
        var blobBinaryData = BinaryData.FromString(
            JsonSerializer.Serialize(order)
        );
        var blobOptions = new BlobUploadOptions
        {
            AccessTier = AccessTier.Cool,
            Metadata = new Dictionary<string, string>
            {
                { "Creator" , "TheCloudShopsMetaData" },
                { "Department", "R&D"  },
                { "Status", "Active"  }
            },
            Tags = new Dictionary<string, string>
            {
                { "CustomerName", order.CustomerName }
            }
        };

        var blobClient = container.GetBlobClient($"customer-{order.Id}");
        _ = await blobClient.UploadAsync(blobBinaryData, blobOptions);
    }

    public static async Task<List<Order>> FindOrder(this BlobServiceClient blobServiceClient, string customerName)
    {
        var taggedBlobs = blobServiceClient.FindBlobsByTagsAsync(
            @$"@container = '{ContainerName}' AND ""CustomerName"" = '{customerName}'"
        );

        var orders = new List<Order>();

        await foreach (var taggedBlob in taggedBlobs)
        {
            Console.WriteLine(
                $"BlobIndex search find BlobName={taggedBlob.BlobName} with tag {taggedBlob.Tags["CustomerName"]}"
            );
            
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(taggedBlob.BlobContainerName);
            var blobClient = blobContainerClient.GetBlobClient(taggedBlob.BlobName);

            var downloadResult = await blobClient.DownloadContentAsync();
            var blobBinaryData = downloadResult.Value.Content;
            
            orders.Add(
                blobBinaryData.ToObjectFromJson<Order>()
            );
        }

        return orders;
    }

    public static void GetContainerMetadata(this BlobServiceClient blobServiceClient)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        var properties = blobContainerClient.GetProperties();

        Console.WriteLine($"Container Name: {blobContainerClient.Name}");

        var metadata = properties.Value.Metadata;
        foreach (var key in metadata.Keys)
            Console.WriteLine($"\tmetadata: {key}={metadata[key]}");
    }
}

public record Order(int Id, string CustomerName);