namespace SalesAnalytics.Domain.Entities;

/// <summary>
/// Detalle de orden (línea de venta) - Entidad de negocio.
/// </summary>
public sealed class OrderDetail
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? TotalPrice { get; set; }
}
