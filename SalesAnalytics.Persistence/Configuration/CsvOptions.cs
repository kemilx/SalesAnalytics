namespace SalesAnalytics.Infrastructure.Configuration;

public sealed class CsvOptions
{
    public string CustomersPath { get; set; } = string.Empty;
    public string ProductsPath { get; set; } = string.Empty;
    public string OrdersPath { get; set; } = string.Empty;
    public string OrderDetailsPath { get; set; } = string.Empty;
}