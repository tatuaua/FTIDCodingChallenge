using System.Text.Json;

class ProductGrouper
{
    private readonly static string fakeStoreUrl = "https://fakestoreapi.com";
    private readonly static string productsEndpoint = "/products";
    private readonly static string productsOutputFileName = "grouped_products.json";

    private readonly static Dictionary<string, List<GroupedProduct>> categoryToProductMap = new();

    static async Task Main(string[] args)
    {
        string? productsString = await GetValueAsync(fakeStoreUrl + productsEndpoint);

        while (productsString == null)
        {
            Console.WriteLine("Error while fetching products, retrying");
            Thread.Sleep(1000);
            productsString = await GetValueAsync(fakeStoreUrl + productsEndpoint);
        }

        Console.WriteLine("Products fetched, deserializing");

        IList<Product>? products = DeserializeProductsJson(productsString);

        Console.WriteLine("Products deserialized, transforming and grouping by category");

        MapProducts(products);

        Console.WriteLine("Products grouped, writing to file");

        WriteProductsToFile();
    }

    /// <summary>
    /// Deserializes JSON string to a list of Product objects.
    /// </summary>
    private static IList<Product> DeserializeProductsJson(string productsString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        IList<Product>? products = JsonSerializer.Deserialize<List<Product>>(productsString, options);

        return products ?? new List<Product>();
    }

    /// <summary>
    /// Groups and sorts products by category.
    /// </summary>
    private static void MapProducts(IList<Product> products)
    {
        foreach (var product in products)
        {
            if (!categoryToProductMap.ContainsKey(product.Category))
            {
                categoryToProductMap.Add(product.Category, new List<GroupedProduct>());
            }

            var list = categoryToProductMap[product.Category];
            list.Add(Transform(product));
            list.Sort((x, y) => x.Price.CompareTo(y.Price)); // Descending; lowest price first
        }
    }

    /// <summary>
    /// Serializes the grouped products to JSON and writes to a file.
    /// </summary>
    private static void WriteProductsToFile()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string jsonString = JsonSerializer.Serialize(categoryToProductMap, options);

        Console.WriteLine(jsonString);

        string binPath = Path.Combine(AppContext.BaseDirectory, productsOutputFileName);

        File.WriteAllText(binPath, jsonString);
    }

    /// <summary>
    /// Transforms a Product object into a GroupedProduct object.
    /// </summary>
    private static GroupedProduct Transform(Product product)
    {
        return new GroupedProduct(product.Id, product.Title, product.Price);
    }

    /// <summary>
    /// Makes an HTTP GET request and retrieves the response as a string.
    /// </summary>
    private static async Task<string?> GetValueAsync(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            string responseString = await reader.ReadToEndAsync();

            if (responseString.Length > 0)
            {
                return responseString;
            }
        }

        return null;
    }

    /// <summary>
    /// Represents a grouped product with simplified fields.
    /// </summary>
    private class GroupedProduct
    {
        public GroupedProduct(int id, string? title, decimal price)
        {
            Id = id;
            Title = title;
            Price = price;
        }

        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Represents a product fetched from the API.
    /// </summary>
    private class Product
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public string? Image { get; set; }
        public Rating? Rating { get; set; }
    }

    /// <summary>
    /// Represents the rating details of a product.
    /// </summary>
    private class Rating
    {
        public double Rate { get; set; }
        public int Count { get; set; }
    }
}
