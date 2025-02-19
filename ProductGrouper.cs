﻿using System.Text.Json;

class ProductGrouper
{
    private readonly static string FAKE_STORE_URL = "https://fakestoreapi.com";
    private readonly static string PRODUCTS_ENDPOINT = "/products";
    private readonly static string PRODUCTS_OUTPUT_FILENAME = "grouped_products.json";

    static async Task Main(string[] args)
    {
        string? productsString = await GetRequest(FAKE_STORE_URL + PRODUCTS_ENDPOINT);

        while (productsString == null)
        {
            Console.WriteLine("Error while fetching products, retrying");
            Thread.Sleep(1000);
            productsString = await GetRequest(FAKE_STORE_URL + PRODUCTS_ENDPOINT);
        }

        Console.WriteLine("Products fetched, deserializing");

        List<Product>? products = Product.FromJSON(productsString);

        Console.WriteLine("Products deserialized, transforming and grouping by category");

        Dictionary<string, List<GroupedProduct>> categoryToProductsMap = MapProducts(products);

        Console.WriteLine("Products grouped, writing to file");

        WriteProductsToFile(categoryToProductsMap);
    }

    /// <summary>
    /// Groups and sorts products by category.
    /// </summary
    private static Dictionary<string, List<GroupedProduct>> MapProducts(List<Product> products)
    {
        Dictionary<string, List<GroupedProduct>> categoryToProductsMap = new();

        foreach (var product in products)
        {
            if (!categoryToProductsMap.ContainsKey(product.Category))
            {
                categoryToProductsMap.Add(product.Category, new List<GroupedProduct>());
            }

            categoryToProductsMap[product.Category].Add(GroupedProduct.Transform(product));
        }

        foreach (var productList in categoryToProductsMap.Values)
        {
            productList.Sort((x, y) => x.Price.CompareTo(y.Price));
        }

        return categoryToProductsMap;
    }

    /// <summary>
    /// Serializes the grouped products to JSON and writes to a file.
    /// </summary>
    private static void WriteProductsToFile(Dictionary<string, List<GroupedProduct>> categoryToProductsMap)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string jsonString = JsonSerializer.Serialize(categoryToProductsMap, options);

        Console.WriteLine(jsonString);

        string binPath = Path.Combine(AppContext.BaseDirectory, PRODUCTS_OUTPUT_FILENAME);

        File.WriteAllText(binPath, jsonString);
    }

    /// <summary>
    /// Makes an HTTP GET request and retrieves the response as a string.
    /// </summary>
    private static async Task<string?> GetRequest(string url)
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
}
