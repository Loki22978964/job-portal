namespace DAL.Interfaces;

public interface IUserRepository : IRepository<User>
{
    
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithRoleAsync(int id, CancellationToken cancellationToken = default);
}
