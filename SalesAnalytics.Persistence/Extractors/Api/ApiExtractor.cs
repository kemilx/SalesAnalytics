using System.Net.Http.Json;
using SalesAnalytics.Application.Abstractions.Extract;

namespace SalesAnalytics.Infrastructure.Extractors.Api;

public sealed class ApiJsonExtractor<T> : IExtractor<T>
{
    public string Name { get; }
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public ApiJsonExtractor(string name, IHttpClientFactory factory, string endpoint)
    {
        Name = name;
        _http = factory.CreateClient("SalesApi");
        _endpoint = endpoint;
    }

    public async Task<IReadOnlyList<T>> ExtractAsync(CancellationToken ct)
        => await _http.GetFromJsonAsync<List<T>>(_endpoint, ct) ?? [];
}