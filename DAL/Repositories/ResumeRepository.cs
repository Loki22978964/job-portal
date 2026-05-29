namespace DAL.Repositories;

public sealed class ResumeRepository : Repository<Resume>, IResumeRepository
{
    private readonly AppDbContext _context;

    public ResumeRepository(AppDbContext context) : base(context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<IReadOnlyList<Resume>> GetByUserIdAsync(
        int userId, CancellationToken cancellationToken = default) =>
        await _context.Resumes
            .Where(r => r.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Resume>> GetAllWithUserAsync(
        CancellationToken cancellationToken = default) =>
        await _context.Resumes
            .Include(r => r.User)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Resume?> GetByIdWithDetailsAsync(
        int id, CancellationToken cancellationToken = default) =>
        await _context.Resumes
            .Include(r => r.User)
            .Include(r => r.Applications)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            .ConfigureAwait(false);
}
