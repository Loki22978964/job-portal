namespace DAL.Entities;

public sealed class User
{
    private readonly List<Resume> _resumes = [];
    private readonly List<Vacancy> _vacancies = [];

    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public IReadOnlyCollection<Resume> Resumes => _resumes;

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies;
}
