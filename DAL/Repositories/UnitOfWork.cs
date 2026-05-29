namespace DAL.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IResumeRepository? _resumes;
    private IVacancyRepository? _vacancies;
    private IUserRepository? _users;
    private IApplicationRepository? _applications;
    private IRepository<Role>? _roles;

    public UnitOfWork(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public IResumeRepository Resumes =>
        _resumes ??= new ResumeRepository(_context);

    public IVacancyRepository Vacancies =>
        _vacancies ??= new VacancyRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IApplicationRepository Applications =>
        _applications ??= new ApplicationRepository(_context);

    public IRepository<Role> Roles =>
        _roles ??= new Repository<Role>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
