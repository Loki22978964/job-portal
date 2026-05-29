namespace DAL.Repositories;

public sealed class VacancyRepository : Repository<Vacancy>, IVacancyRepository
{
    private readonly AppDbContext _context;

    public VacancyRepository(AppDbContext context) : base(context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<IReadOnlyList<Vacancy>> GetByUserIdAsync(
        int userId, CancellationToken cancellationToken = default) =>
        await _context.Vacancies
            .Where(v => v.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Vacancy>> GetAllWithUserAsync(
        CancellationToken cancellationToken = default) =>
        await _context.Vacancies
            .Include(v => v.User)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Vacancy?> GetByIdWithDetailsAsync(
        int id, CancellationToken cancellationToken = default) =>
        await _context.Vacancies
            .Include(v => v.User)
            .Include(v => v.Applications)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            .ConfigureAwait(false);
}
