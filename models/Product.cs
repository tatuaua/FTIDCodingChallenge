using System.Text.Json;

/// <summary>
/// Represents a product fetched from the API.
/// </summary>
class Product
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; }
    public string? Image { get; set; }
    public Rating? Rating { get; set; }

    /// <summary>
    /// Deserializes JSON string to a list of Product objects.
    /// </summary>
    public static List<Product> FromJSON(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        List<Product>? products = null;

        try
        {
            products = JsonSerializer.Deserialize<List<Product>>(jsonString, options);

        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("Exception: Product deserializer received null string");

        }
        catch (JsonException)
        {
            Console.WriteLine("Exception: Product deserializer received invalid json");
        }

        return products ?? new List<Product>();
    }

}
