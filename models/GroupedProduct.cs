/// <summary>
/// Represents a grouped product with simplified fields.
/// </summary>
class GroupedProduct
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

    /// <summary>
    /// Transforms a Product object into a GroupedProduct object.
    /// </summary>
    public static GroupedProduct Transform(Product product)
    {
        return new GroupedProduct(product.Id, product.Title, product.Price);
    }

}