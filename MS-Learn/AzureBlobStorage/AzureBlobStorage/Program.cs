using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobStorage;

public static class Program
{
    private const string StorageAccountConnectionString = "[CONNECTION-STRING]";

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Azure Blob Storage exercise");

        var fileName = await CreateLocalFile();

        try
        {
            var blobServiceClient = CreateBlobServiceClient();
            var blobContainerClient = await CreateBlobContainerClient(blobServiceClient);
            await AddContainerMetadata(blobContainerClient);
            await ReadContainerProperties(blobContainerClient);
            
            var blobClient = await CreateBlob(blobContainerClient, fileName);
            await ListBlobs(blobContainerClient);
            await DownloadBlob(blobClient, fileName);
            await DeleteBlobContainer(blobContainerClient, fileName);
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
            Console.WriteLine($"Exception message: {e.Message}");
        }

        Console.WriteLine("Press enter to exit the sample application.");
        _ = Console.ReadLine();
    }

    /// <summary>
    /// Create a client that can authenticate with a connection string.
    /// </summary>
    private static BlobServiceClient CreateBlobServiceClient()
        => new(StorageAccountConnectionString);

    /// <summary>
    /// Create the container and return a container client object.
    /// </summary>
    private static async Task<BlobContainerClient> CreateBlobContainerClient(
        BlobServiceClient blobServiceClient)
    {
        var containerName = $"mjoyblob{Guid.NewGuid():D}";
        var blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

        Console.WriteLine($"A container named '{containerName}' has been created.");
        Console.WriteLine("Take a minute and verify in the portal.");
        Console.WriteLine("Next a file will be created and uploaded to the container.");
        Console.WriteLine("Press 'Enter' to continue.");
        _ = Console.ReadLine();

        return blobContainerClient;
    }

    public static async Task AddContainerMetadata(BlobContainerClient container)
    {
        var metadata = new Dictionary<string, string>
        {
            { "docType", "textDocuments" },
            { "category", "guidance" }
        };

        // Set the container's metadata.
        _ = await container.SetMetadataAsync(metadata);
    }

    private static async Task ReadContainerProperties(BlobContainerClient container)
    {
        // Fetch some container properties and write out their values.
        var properties = await container.GetPropertiesAsync();
        
        Console.WriteLine($"Properties for container {container.Uri}");
        Console.WriteLine($"Public access level: {properties.Value.PublicAccess}");
        Console.WriteLine($"Last modified time in UTC: {properties.Value.LastModified}");

        Console.WriteLine("Container metadata:");
        foreach (var metadataItem in properties.Value.Metadata)
        {
            Console.WriteLine($"\t{metadataItem.Key}: {metadataItem.Value}");
        }
    }

    /// <summary>
    /// Get a reference to the blob.
    /// </summary>
    private static async Task<BlobClient> CreateBlob(
        BlobContainerClient blobContainerClient, string fileName)
    {
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}", blobClient.Uri);

        using var uploadFileStream = File.OpenRead(
            ToLocalDataFilePath(fileName));
        _ = await blobClient.UploadAsync(uploadFileStream);

        Console.WriteLine("The file was uploaded. We'll verify by listing the blobs next.");
        Console.WriteLine("Press 'Enter' to continue.");
        _ = Console.ReadLine();

        return blobClient;
    }

    /// <summary>
    /// List blobs in the container.
    /// </summary>
    private static async Task ListBlobs(
        BlobContainerClient blobContainerClient)
    {
        Console.WriteLine("Listing blobs...");
        await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
        {
            Console.WriteLine("\t" + blobItem.Name);
        }

        Console.WriteLine("You can also verify by looking inside the container in the portal.");
        Console.WriteLine("Next the blob will be downloaded with an altered file name.");
        Console.WriteLine("Press 'Enter' to continue.");
        _ = Console.ReadLine();
    }

    /// <summary>
    /// Download the blob to a local file.
    /// </summary>
    private static async Task DownloadBlob(
        BlobClient blobClient, string fileName)
    {
        var downloadFilePath = ToLocalDownloadFilePath(fileName);

        Console.WriteLine("Downloading blob to\n\t{0}", downloadFilePath);

        // Can't use 'var' here. We explicitly require the 'BlobDownloadInfo' class.
        BlobDownloadInfo download = await blobClient.DownloadAsync();

        using var downloadFileStream = File.OpenWrite(downloadFilePath);
        await download.Content.CopyToAsync(downloadFileStream);
        
        Console.WriteLine("Locate the local file in the data directory created earlier to verify it was downloaded.");
        Console.WriteLine("The next step is to delete the container and local files.");
        Console.WriteLine("Press 'Enter' to continue.");
        _ = Console.ReadLine();
    }

    /// <summary>
    /// Delete the container and clean up local files created.
    /// </summary>
    private static async Task DeleteBlobContainer(
        BlobContainerClient blobContainerClient, string fileName)
    {
        Console.WriteLine("Deleting blob container...");
        _ = await blobContainerClient.DeleteAsync();

        Console.WriteLine("Deleting the local source and downloaded files...");

        File.Delete(
            ToLocalDataFilePath(fileName));
        File.Delete(
            ToLocalDownloadFilePath(fileName));

        Console.WriteLine("Finished cleaning up.");
    }
    /// <summary>
    /// Create a local file in the ./data/ directory for uploading and downloading
    /// </summary>
    private static async Task<string> CreateLocalFile()
    {
        var fileName = $"mjoyfile{Guid.NewGuid():D}.txt";

        await File.WriteAllTextAsync(
            ToLocalDataFilePath(fileName), "Hello, World!");

        return fileName;
    }

    /// <summary>
    /// Convert a filename to the local file path.
    /// </summary>
    private static string ToLocalDataFilePath(string fileName)
        => Path.Combine("./data/", fileName);

    private static string ToLocalDownloadFilePath(string fileName)
        => ToLocalDataFilePath(fileName).Replace(".txt", "DOWNLOADED.txt");
}