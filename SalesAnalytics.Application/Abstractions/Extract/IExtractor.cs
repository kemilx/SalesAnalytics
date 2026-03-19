namespace SalesAnalytics.Application.Abstractions.Extract;

public interface IExtractor<T>
{
    Task<IReadOnlyList<T>> ExtractAsync(CancellationToken cancellationToken = default);
}