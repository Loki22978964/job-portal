namespace DAL.Interfaces;

public interface IResumeRepository : IRepository<Resume>
{
    
    Task<IReadOnlyList<Resume>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resume>> GetAllWithUserAsync(CancellationToken cancellationToken = default);

    Task<Resume?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
