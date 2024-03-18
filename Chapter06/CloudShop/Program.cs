using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShop;

public static class Program
{
    public const string ConnectionString = "";
    public const string ContainerName = "cloudshop";
    public const string LogoFileName = "logo.png";

    private static async Task Main(string[] args)
    {
        var blobServiceClient = new BlobServiceClient(ConnectionString);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        _ = await blobContainerClient.CreateIfNotExistsAsync();

        await blobContainerClient.TryUploadBlob(LogoFileName, GetFileDirectory(LogoFileName));

        //Wait one second to be sure the file was uploaded.
        Thread.Sleep(1000);

        await blobContainerClient.TryDownloadBlob(LogoFileName, GetFileDirectory($"downloaded-{LogoFileName}"));

        Console.WriteLine($"Press any key to delete file '{LogoFileName}' on blob?");
        _ = Console.ReadKey();

        await blobContainerClient.TryDeleteBlob(LogoFileName);
    }

    public static string GetFileDirectory(string fileName)
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", fileName);

    public static async Task TryDeleteBlob(this BlobContainerClient blobContainer, string blobName)
    {
        var blobClient = blobContainer.GetBlobClient(blobName);
        _ = await blobClient.DeleteIfExistsAsync();
        Console.WriteLine($"Deleted blob '{blobName}'");
    }

    public static Task TryUploadBlob(this BlobContainerClient blobContainer, string blobName, string filePath)
    {
        using var file = File.OpenRead(filePath);
        var blobClient = blobContainer.GetBlobClient(blobName);
        return blobClient
            .UploadAsync(file, true)
            .ContinueWith((result) => Console.WriteLine($"File uploaded with status {result.Status}"));
    }

    public static Task TryDownloadBlob(this BlobContainerClient blobContainer, string blobName, string filePath)
    {
        using var file = File.OpenWrite(filePath);
        var blobClient = blobContainer.GetBlobClient(blobName);
        return blobClient
            .DownloadToAsync(file)
            .ContinueWith((result) => Console.WriteLine($"File downloaded with status {result.Status}"));
    }
}