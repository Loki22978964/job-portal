namespace DAL.Repositories;

public sealed class ApplicationRepository : Repository<Application>, IApplicationRepository
{
    private readonly AppDbContext _context;

    public ApplicationRepository(AppDbContext context) : base(context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<IReadOnlyList<Application>> GetByResumeIdAsync(
        int resumeId, CancellationToken cancellationToken = default) =>
        await _context.Applications
            .Include(a => a.Vacancy).ThenInclude(v => v!.User)
            .Where(a => a.ResumeId == resumeId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Application>> GetByVacancyIdAsync(
        int vacancyId, CancellationToken cancellationToken = default) =>
        await _context.Applications
            .Include(a => a.Resume).ThenInclude(r => r!.User)
            .Where(a => a.VacancyId == vacancyId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> ExistsAsync(
        int resumeId, int vacancyId, CancellationToken cancellationToken = default) =>
        await _context.Applications
            .AnyAsync(a => a.ResumeId == resumeId && a.VacancyId == vacancyId, cancellationToken)
            .ConfigureAwait(false);
}
