namespace PRN232.Lab1.Repositories.Abstractions;

public sealed record RepositoryPage<TEntity>(IReadOnlyList<TEntity> Items, int TotalItems, int Page, int PageSize);

public interface IRepository<TEntity> where TEntity : class
{
    Task<RepositoryPage<TEntity>> GetPageAsync(string? search, string? sort, int page, int size, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(int id, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}