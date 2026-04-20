namespace SalesAnalytics.Domain.Entities;

/// <summary>
/// Orden / Factura (cabecera) - Entidad de negocio.
/// </summary>
public sealed class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
}
