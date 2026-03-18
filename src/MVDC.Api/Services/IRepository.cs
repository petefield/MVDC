namespace MVDC.Api.Services;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T item, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(string id, T item, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
