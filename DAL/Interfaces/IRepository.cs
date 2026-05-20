namespace DAL.Interfaces;

public interface IRepository<T> where T : class
{
    
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
