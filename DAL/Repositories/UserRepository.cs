namespace DAL.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) : base(context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<User?> GetByIdWithRoleAsync(
        int id, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            .ConfigureAwait(false);
}
