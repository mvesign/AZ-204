using Microsoft.Azure.Cosmos;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AzureCosmosDb;

public class Program
{
    private const string EndpointUri = "https://mjoy-cosmos-acc-westeurope.documents.azure.com:443/";
    private const string PrimaryKey = "[PRIMARY-KEY]";
    private const string DatabaseId = "mjoyDatabase";
    private const string ContainerId = "mjoyContainer";
    private const string PartitionKey = "/LastName";

    private static CosmosClient? CosmosClient = null;
    private static Database? Database = null;
    private static Container? Container = null;

    private static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Beginning operations...");
            await SetupCosmosDb();

            Console.WriteLine("Creating items...");
            await CreateItem(1, "Henk", "de Tank");
            await CreateItem(2, "Piet", "Friet");
            await CreateItem(3, "Riet", "Friet");
        }
        catch (CosmosException exception)
        {
            var baseException = exception.GetBaseException();
            Console.WriteLine($"{exception.StatusCode} error occurred: {exception}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error: {exception}");
        }
        finally
        {
            Console.WriteLine("End of program, press any key to exit.");
            _ = Console.ReadKey();
        }
    }

    public static async Task SetupCosmosDb()
    {
        CosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
        Console.WriteLine($"Created Cosmos client: {EndpointUri}");

        Database = await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
        Console.WriteLine($"Created Database: {Database.Id}");

        Container = await Database.CreateContainerIfNotExistsAsync(ContainerId, PartitionKey);
        Console.WriteLine($"Created Container: {Container.Id}");
    }

    public static async Task CreateItem(int id, string firstName, string lastName)
    {
        ArgumentNullException.ThrowIfNull(Container);

        var item = await Container!.CreateItemAsync(new Person(id, firstName, lastName));
        Console.WriteLine($"Created Item: {item}");
    }
}

public record Person([property: JsonPropertyName("id")] int id, string FirstName, string Lastname);