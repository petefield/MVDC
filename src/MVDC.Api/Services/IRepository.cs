namespace MVDC.Api.Services;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T> CreateAsync(T item);
    Task<T> UpdateAsync(string id, T item);
    Task DeleteAsync(string id);
}
