using Azure;
using Azure.Data.Tables;
using System;
using System.Threading.Tasks;

namespace AzureTableStorage;

public static class Program
{
    private static async Task Main(string[] args)
    {
        // Pre-setup some variables
        var connectionString = "";
        var tableName = "customerscode";

        var client = new TableClient(connectionString, tableName);
        _ = await client.CreateIfNotExistsAsync();

        await CreateOrUpsertCustomer(client, "WingTipToys", "ReSellers", true);
        await CreateOrUpsertCustomer(client, "ADatum", "ReSellers", true);
        await CreateOrUpsertCustomer(client, "FabricanFuber", "Sellers", false);

        await QueryCustomers(client, "PartitionKey eq 'ReSellers'");
        await QueryCustomers(client, "IsActive eq false");

        Console.WriteLine("Press enter to start deletion...");
        _ = Console.ReadLine();

        await DeleteEntityAndTable(client, "ADatum", "ReSellers");
    }

    private static async Task CreateOrUpsertCustomer(
        TableClient tableClient, string name, string type, bool IsActive
    )
    {
        var entity = new TableEntity(type, name)
        {
            { "IsActive", IsActive },
        };

        try
        {
            // First try to get an existing entity.
            var existingEntity = tableClient.GetEntity<TableEntity>(type, name);

            _ = await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
            Console.WriteLine($"Upsert entity '{name}' of type '{type}'");
        }
        catch(RequestFailedException)
        {
            // If no entity exists, a RequestFailedException will be thrown.
            _ = await tableClient.AddEntityAsync(entity);
            Console.WriteLine($"Add entity '{name}' of type '{type}'");
        }
    }

    private static async Task QueryCustomers(TableClient tableClient, string filter)
    {
        Console.WriteLine($"Query filter: {filter}");

        var entities = tableClient.QueryAsync<TableEntity>(filter);

        await foreach (var entity in entities)
            Console.WriteLine($"{entity.GetString("RowKey")}: {entity.GetBoolean("IsActive")}");
    }

    private static async Task DeleteEntityAndTable(TableClient client, string name, string type)
    {
        // Delete an entity
        _ = await client.DeleteEntityAsync(type, name);
        Console.WriteLine($"Deleted entity '{name}' of type '{type}'");
        
        Console.WriteLine("Press enter to also delete table...");
        _ = Console.ReadLine();

        // Delete the table
        _ = await client.DeleteAsync();
        Console.WriteLine("Deleted table");
    }
}
