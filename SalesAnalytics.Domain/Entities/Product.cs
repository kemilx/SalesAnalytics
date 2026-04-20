namespace SalesAnalytics.Domain.Entities;

/// <summary>
/// Producto (Entidad de negocio).
/// </summary>
public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int? Stock { get; set; }
}
