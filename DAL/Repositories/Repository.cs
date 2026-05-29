namespace DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbSet.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        T? entity = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is not null)
            _dbSet.Remove(entity);
    }
}
