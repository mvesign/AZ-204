using CosmosDB.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDB;

public static class Program
{
    private static readonly string EndpointUri = "";
    private static readonly string PrimaryKey = "";

    private static CosmosClient? CosmosClient;
    private static Database? Database;
    private static Container? Container;

    private const string DatabaseName = "mjoycosmosdb";
    private const string ContainerName = "CloudShops";

    private static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Beginning operations...");

            // Create required instances
            CosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            
            Database = (Database)await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName);
            Console.WriteLine("Created Database: {0}", Database.Id);

            Container = (Container)await Database.CreateContainerIfNotExistsAsync(ContainerName, "/OrderAddress/City");
            Console.WriteLine("Created Container: {0}", Container.Id);

            // 01: Add items to the container
            //await SeedContainerWithItems();

            // 02: Query items and adjust an item.
            // await QueryItemsAsync();
            // await ReplaceFamilyItem("o3", "Redmond");

            // 03: Add item with pre-trigger
            await AddItemsToContainerWithPreTrigger();
        }
        catch (CosmosException exception)
        {
            var baseException = exception.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", exception.StatusCode, exception);
        }
        catch (Exception exception)
        {
            Console.WriteLine("Error: {0}", exception);
        }
        finally
        {
            Console.WriteLine("End of demo, press any key to exit.");
            _ = Console.ReadKey();
        }
    }

    #region Seed

    private static async Task SeedContainerWithItems()
    {
        var product1 = new Product("Book");
        var product2 = new Product("Food");
        var product3 = new Product("Coffee");

        //create orders
        await CreateDocumentsIfNotExists(
            new Order()
            {
                id = "o1",
                OrderNumber = "NL-21",
                OrderCustomer = new("Level4you", true),
                OrderAddress = new("WA", "King", "Seattle"),
                OrderItems =
                [
                    new(product1, 7),
                    new(product3, 1)
                ]
            }
        );
        await CreateDocumentsIfNotExists(
            new Order()
            {
                id = "o2",
                OrderNumber = "NL-22",
                OrderCustomer = new("Channel-9", false),
                OrderAddress = new("WA", "King", "Redmond"),
                OrderItems =
                [
                    new(product3, 99),
                    new(product2, 5),
                    new(product1, 1)
                ]
            }
        );
        await CreateDocumentsIfNotExists(
            new Order()
            {
                id = "o3",
                OrderNumber = "NL-23",
                OrderCustomer = new("UpperLevel", true),
                OrderAddress = new("WA", "King", "Redmond"),
                OrderItems =
                [
                    new(product2,  2)
                ]
            }
        );
    }

    #endregion

    #region Query

    private static async Task QueryItemsAsync()
    {
        var sqlQueryText = "SELECT * FROM c";

        Console.WriteLine("Running query: {0}", sqlQueryText);

        var queryDefinition = new QueryDefinition(sqlQueryText);
        var queryResultSetIterator = Container!.GetItemQueryIterator<Order>(queryDefinition);

        var families = new List<Order>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (var order in currentResultSet)
            {
                families.Add(order);
                Console.WriteLine("\tRead {0}", order.OrderNumber);
            }

            Console.WriteLine("Select orders. Operation consumed {0} RUs.\n", currentResultSet.RequestCharge);
        }
    }

    #endregion

    #region Adjust

    private static async Task ReplaceFamilyItem(string id, string partition)
    {
        var orderResponse = await Container!.ReadItemAsync<Order>(id, new PartitionKey(partition));
        var itemBody = orderResponse.Resource;

        // Update registration status from false to true
        var oldNumber = itemBody.OrderNumber;
        itemBody.OrderNumber = "NL-77";

        // Replace the item with the updated content
        orderResponse = await Container.ReplaceItemAsync(
            itemBody, itemBody.id, new PartitionKey(itemBody.OrderAddress!.City)
        );
        Console.WriteLine(
            "Updated Order Number {0}.\n Number is now: {1}\n Operation consumed {2} RUs",
            oldNumber, itemBody.OrderNumber, orderResponse.RequestCharge
        );
    }

    #endregion

    #region Trigger

    private static async Task AddItemsToContainerWithPreTrigger()
        => await CreateDocumentsIfNotExists(new Order()
        {
            id = "o11",
            OrderNumber = "NL-25",
            OrderCustomer = null,  // the customer is not defined.
            OrderAddress = new("WA", "King", "Seattle"),
            OrderItems =
            [
                new(
                    new("Covid"), 1
                )
            ]
        });

    #endregion

    private static async Task CreateDocumentsIfNotExists(Order order) 
    {
        try
        {
            var readResponse = await Container!.ReadItemAsync<Order>(
                order.id, new PartitionKey(order.OrderAddress!.City)
                
            );
            Console.WriteLine("Item in database with id: {0} already exists\n", readResponse.Resource.id);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var createResponse = await Container!.CreateItemAsync(
                order, new PartitionKey(order.OrderAddress!.City),
                new ItemRequestOptions() { PreTriggers = [ "validateOrder" ] }
            );
            Console.WriteLine(
                "Created item in database with id: {0} Operation consumed {1} RUs.",
                createResponse.Resource.id, createResponse.RequestCharge
            );
        }
    }
}