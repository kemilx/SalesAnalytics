namespace SalesAnalytics.Application.Models.Raw;

public sealed class OrderDetailRaw
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal? TotalPrice { get; set; }
}