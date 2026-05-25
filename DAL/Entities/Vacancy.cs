namespace DAL.Entities;

public sealed class Vacancy
{
    private readonly List<Application> _applications = [];

    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    public string RequiredSkills { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public IReadOnlyCollection<Application> Applications => _applications;
}
