using System.Data;
using Microsoft.Data.SqlClient;
using SalesAnalytics.Application.Abstractions.Staging;
using SalesAnalytics.Application.Models.Raw;

namespace SalesAnalytics.Infrastructure.Staging;

public sealed class SqlStagingWriter : IStagingWriter
{
    private readonly string _connectionString;

    public SqlStagingWriter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task ClearStagingAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           DELETE FROM stg.OrderDetails;
                           DELETE FROM stg.Orders;
                           DELETE FROM stg.Products;
                           DELETE FROM stg.Customers;
                           """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, conn)
        {
            CommandTimeout = 120
        };

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task WriteCustomersAsync(IEnumerable<CustomerRaw> rows, CancellationToken cancellationToken = default)
        => BulkInsertAsync("stg.Customers", BuildCustomersTable(rows), cancellationToken);

    public Task WriteProductsAsync(IEnumerable<ProductRaw> rows, CancellationToken cancellationToken = default)
        => BulkInsertAsync("stg.Products", BuildProductsTable(rows), cancellationToken);

    public Task WriteOrdersAsync(IEnumerable<OrderRaw> rows, CancellationToken cancellationToken = default)
        => BulkInsertAsync("stg.Orders", BuildOrdersTable(rows), cancellationToken);

    public Task WriteOrderDetailsAsync(IEnumerable<OrderDetailRaw> rows, CancellationToken cancellationToken = default)
        => BulkInsertAsync("stg.OrderDetails", BuildOrderDetailsTable(rows), cancellationToken);

    public async Task RunWarehouseProceduresAsync(CancellationToken cancellationToken = default)
    {
        // En esta etapa (carga de dimensiones), ejecutamos solo los SP de dimensiones.
        // La carga de hechos (FactVentas) puede ejecutarse en la siguiente práctica.
        string[] procedures =
        {
            "dbo.sp_PoblarDimFecha",
            "dbo.sp_PoblarDimCliente",
            "dbo.sp_PoblarDimProducto"
        };

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        foreach (var procedure in procedures)
        {
            await using var cmd = new SqlCommand(procedure, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task BulkInsertAsync(string destinationTable, DataTable table, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG] Abriendo conexión para {destinationTable}...");

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        Console.WriteLine($"[DEBUG] Conexión abierta para {destinationTable}. Filas: {table.Rows.Count}");

        using var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, null)
        {
            DestinationTableName = destinationTable,
            BatchSize = 500,
            BulkCopyTimeout = 0
        };

        foreach (DataColumn column in table.Columns)
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        Console.WriteLine($"[DEBUG] Iniciando bulk insert en {destinationTable}...");
        await bulk.WriteToServerAsync(table, cancellationToken);
        Console.WriteLine($"[DEBUG] Bulk insert terminado en {destinationTable}.");
    }

    private static DataTable BuildCustomersTable(IEnumerable<CustomerRaw> rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("CustomerID", typeof(int));
        dt.Columns.Add("FirstName", typeof(string));
        dt.Columns.Add("LastName", typeof(string));
        dt.Columns.Add("Email", typeof(string));
        dt.Columns.Add("Phone", typeof(string));
        dt.Columns.Add("City", typeof(string));
        dt.Columns.Add("Country", typeof(string));

        foreach (var r in rows)
            dt.Rows.Add(r.CustomerID, r.FirstName, r.LastName, r.Email, r.Phone, r.City, r.Country);

        return dt;
    }

    private static DataTable BuildProductsTable(IEnumerable<ProductRaw> rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("ProductID", typeof(int));
        dt.Columns.Add("ProductName", typeof(string));
        dt.Columns.Add("Category", typeof(string));
        dt.Columns.Add("Price", typeof(decimal));
        dt.Columns.Add("Stock", typeof(int));

        foreach (var r in rows)
            dt.Rows.Add(r.ProductID, r.ProductName, r.Category, r.Price, (object?)r.Stock ?? DBNull.Value);

        return dt;
    }

    private static DataTable BuildOrdersTable(IEnumerable<OrderRaw> rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("OrderID", typeof(int));
        dt.Columns.Add("CustomerID", typeof(int));
        dt.Columns.Add("OrderDate", typeof(DateTime));
        dt.Columns.Add("Status", typeof(string));

        foreach (var r in rows)
            dt.Rows.Add(r.OrderID, r.CustomerID, r.OrderDate.Date, r.Status);

        return dt;
    }

    private static DataTable BuildOrderDetailsTable(IEnumerable<OrderDetailRaw> rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("OrderID", typeof(int));
        dt.Columns.Add("ProductID", typeof(int));
        dt.Columns.Add("Quantity", typeof(int));
        dt.Columns.Add("TotalPrice", typeof(decimal));

        foreach (var r in rows)
            dt.Rows.Add(r.OrderID, r.ProductID, r.Quantity, (object?)r.TotalPrice ?? DBNull.Value);

        return dt;
    }
}