namespace SalesAnalytics.Application.Models.Raw;

public sealed class ProductRaw
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int? Stock { get; set; }
}