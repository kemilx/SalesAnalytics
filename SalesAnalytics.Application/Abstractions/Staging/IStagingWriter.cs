using SalesAnalytics.Application.Models.Raw;

namespace SalesAnalytics.Application.Abstractions.Staging;

public interface IStagingWriter
{
    Task ClearStagingAsync(CancellationToken cancellationToken = default);

    Task WriteCustomersAsync(IEnumerable<CustomerRaw> rows, CancellationToken cancellationToken = default);
    Task WriteProductsAsync(IEnumerable<ProductRaw> rows, CancellationToken cancellationToken = default);
    Task WriteOrdersAsync(IEnumerable<OrderRaw> rows, CancellationToken cancellationToken = default);
    Task WriteOrderDetailsAsync(IEnumerable<OrderDetailRaw> rows, CancellationToken cancellationToken = default);

    Task RunWarehouseProceduresAsync(CancellationToken cancellationToken = default);
}