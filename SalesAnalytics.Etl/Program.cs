using Microsoft.Extensions.Options;
using SalesAnalytics.Application.Abstractions.Extract;
using SalesAnalytics.Application.Abstractions.Staging;
using SalesAnalytics.Application.Models.Raw;
using SalesAnalytics.Etl;
using SalesAnalytics.Infrastructure.Configuration;
using SalesAnalytics.Infrastructure.Extractors.Csv;
using SalesAnalytics.Infrastructure.Staging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CsvOptions>(
    builder.Configuration.GetSection("Csv"));

builder.Services.AddSingleton<IExtractor<CustomerRaw>>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var opt = sp.GetRequiredService<IOptions<CsvOptions>>().Value;
    var path = Path.Combine(env.ContentRootPath, opt.CustomersPath);
    return new CsvFileExtractor<CustomerRaw>(path);
});

builder.Services.AddSingleton<IExtractor<ProductRaw>>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var opt = sp.GetRequiredService<IOptions<CsvOptions>>().Value;
    var path = Path.Combine(env.ContentRootPath, opt.ProductsPath);
    return new CsvFileExtractor<ProductRaw>(path);
});

builder.Services.AddSingleton<IExtractor<OrderRaw>>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var opt = sp.GetRequiredService<IOptions<CsvOptions>>().Value;
    var path = Path.Combine(env.ContentRootPath, opt.OrdersPath);
    return new CsvFileExtractor<OrderRaw>(path);
});

builder.Services.AddSingleton<IExtractor<OrderDetailRaw>>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var opt = sp.GetRequiredService<IOptions<CsvOptions>>().Value;
    var path = Path.Combine(env.ContentRootPath, opt.OrderDetailsPath);
    return new CsvFileExtractor<OrderDetailRaw>(path);
});

builder.Services.AddSingleton<IStagingWriter>(sp =>
{
    var cs = builder.Configuration.GetConnectionString("DwhDb")
             ?? throw new InvalidOperationException("No se encontró ConnectionStrings:DwhDb");
    return new SqlStagingWriter(cs);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();