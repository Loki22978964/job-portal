namespace DAL.Entities;

[SuppressMessage("Naming", "CA1716:IdentifiersShouldNotMatchKeywords",
    Justification = "'Resume' is the canonical domain entity name required by the specification.")]
public sealed class Resume
{
    private readonly List<Application> _applications = [];

    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Skills { get; set; } = string.Empty;

    public decimal ExpectedSalary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public IReadOnlyCollection<Application> Applications => _applications;
}
