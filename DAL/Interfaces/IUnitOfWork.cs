namespace DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    
    IResumeRepository Resumes { get; }

    IVacancyRepository Vacancies { get; }

    IUserRepository Users { get; }

    IApplicationRepository Applications { get; }

    IRepository<Role> Roles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
