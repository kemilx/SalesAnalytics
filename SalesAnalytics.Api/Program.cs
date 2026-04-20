using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok("API OK"));

string connStr = builder.Configuration.GetConnectionString("Dwh")!;

app.MapGet("/api/customers", async () =>
{
    var list = new List<object>();

    using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    var cmd = new SqlCommand("""
        SELECT CustomerID, FirstName, LastName, Email, Phone, City, Country
        FROM stg.Customers
    """, conn);

    using var rd = await cmd.ExecuteReaderAsync();
    while (await rd.ReadAsync())
    {
        list.Add(new
        {
            customerId = rd.GetInt32(0),
            firstName = rd.GetString(1),
            lastName = rd.GetString(2),
            email = rd.IsDBNull(3) ? null : rd.GetString(3),
            phone = rd.IsDBNull(4) ? null : rd.GetString(4),
            city = rd.IsDBNull(5) ? null : rd.GetString(5),
            country = rd.IsDBNull(6) ? null : rd.GetString(6)
        });
    }

    return Results.Ok(list);
});

app.MapGet("/api/products", async () =>
{
    var list = new List<object>();

    using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    var cmd = new SqlCommand("""
        SELECT ProductID, ProductName, Category, Price, Stock
        FROM stg.Products
    """, conn);

    using var rd = await cmd.ExecuteReaderAsync();
    while (await rd.ReadAsync())
    {
        list.Add(new
        {
            productId = rd.GetInt32(0),
            productName = rd.GetString(1),
            category = rd.IsDBNull(2) ? null : rd.GetString(2),
            price = rd.GetDecimal(3),
            stock = rd.IsDBNull(4) ? (int?)null : rd.GetInt32(4)
        });
    }

    return Results.Ok(list);
});

app.Run();