using System.Globalization;
using CsvHelper;
using SalesAnalytics.Application.Abstractions.Extract;

namespace SalesAnalytics.Infrastructure.Extractors.Csv;

public sealed class CsvFileExtractor<T> : IExtractor<T>
{
    private readonly string _filePath;

    public CsvFileExtractor(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IReadOnlyList<T>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"No se encontró el archivo CSV: {_filePath}");

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<T>().ToList();
        return await Task.FromResult(records);
    }
}