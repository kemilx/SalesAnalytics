namespace SalesAnalytics.Application.Models.Raw;

public sealed class OrderRaw
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
}