using System.Text.Json;
using System.Text.Json.Serialization;

namespace CosmosDB.Models;

public record Address(string State, string County, string City);

public record Customer(string Name, bool IsActive);

public class Order 
{
    public string? id { get; set; }
    public string? OrderNumber { get; set; }
    public Address? OrderAddress { get; set; }
    public Customer? OrderCustomer { get; set; }
    public OrderItem[] OrderItems { get; set; } = [];

    public override string ToString()
        => JsonSerializer.Serialize(this);
}

public record OrderItem(Product ProductItem, int Count);

public record Product(string ProductName);