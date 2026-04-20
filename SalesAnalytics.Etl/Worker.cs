using System.Diagnostics;
using SalesAnalytics.Application.Abstractions.Extract;
using SalesAnalytics.Application.Abstractions.Staging;
using SalesAnalytics.Application.Models.Raw;

namespace SalesAnalytics.Etl;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IExtractor<CustomerRaw> _customers;
    private readonly IExtractor<ProductRaw> _products;
    private readonly IExtractor<OrderRaw> _orders;
    private readonly IExtractor<OrderDetailRaw> _details;
    private readonly IStagingWriter _staging;

    public Worker(
        ILogger<Worker> logger,
        IExtractor<CustomerRaw> customers,
        IExtractor<ProductRaw> products,
        IExtractor<OrderRaw> orders,
        IExtractor<OrderDetailRaw> details,
        IStagingWriter staging)
    {
        _logger = logger;
        _customers = customers;
        _products = products;
        _orders = orders;
        _details = details;
        _staging = staging;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var total = Stopwatch.StartNew();

        _logger.LogInformation("ETL Extract iniciado.");

        await _staging.ClearStagingAsync(stoppingToken);

        await RunOne("Customers", _customers, rows => _staging.WriteCustomersAsync(rows, stoppingToken), stoppingToken);
        await RunOne("Products", _products, rows => _staging.WriteProductsAsync(rows, stoppingToken), stoppingToken);
        await RunOne("Orders", _orders, rows => _staging.WriteOrdersAsync(rows, stoppingToken), stoppingToken);

        var detailsSw = Stopwatch.StartNew();

        _logger.LogInformation("Extrayendo OrderDetails...");
        var detailRows = await _details.ExtractAsync(stoppingToken);
        _logger.LogInformation("OrderDetails: {Count} registros extraídos.", detailRows.Count);

        var normalizedDetails = NormalizeOrderDetails(detailRows);
        _logger.LogInformation(
            "OrderDetails: {Duplicates} duplicados procesados. {FinalCount} registros listos para cargar.",
            detailRows.Count - normalizedDetails.Count,
            normalizedDetails.Count);

        await _staging.WriteOrderDetailsAsync(normalizedDetails, stoppingToken);

        detailsSw.Stop();
        _logger.LogInformation("OrderDetails: carga finalizada en {Ms} ms.", detailsSw.ElapsedMilliseconds);

        _logger.LogInformation("Ejecutando procedures del DW...");
        await _staging.RunWarehouseProceduresAsync(stoppingToken);

        total.Stop();
        _logger.LogInformation("ETL Extract terminado en {ms} ms.", total.ElapsedMilliseconds);
    }

    private static IReadOnlyList<OrderDetailRaw> NormalizeOrderDetails(IReadOnlyList<OrderDetailRaw> rows)
    {
        return rows
            .GroupBy(x => new { x.OrderID, x.ProductID })
            .Select(g => new OrderDetailRaw
            {
                OrderID = g.Key.OrderID,
                ProductID = g.Key.ProductID,
                Quantity = g.Sum(x => x.Quantity),
                TotalPrice = g.All(x => x.TotalPrice is null)
                    ? null
                    : g.Sum(x => x.TotalPrice ?? 0m)
            })
            .ToList();
    }

    private async Task RunOne<T>(
        string name,
        IExtractor<T> extractor,
        Func<IReadOnlyList<T>, Task> writer,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Extrayendo {Name}...", name);
        var rows = await extractor.ExtractAsync(cancellationToken);

        _logger.LogInformation("{Name}: {Count} registros extraídos.", name, rows.Count);

        await writer(rows);

        sw.Stop();
        _logger.LogInformation("{Name}: carga finalizada en {Ms} ms.", name, sw.ElapsedMilliseconds);
    }

}