namespace DAL.Entities;

public sealed class Application
{
    
    public int Id { get; set; }

    public int ResumeId { get; set; }

    public Resume Resume { get; set; } = null!;

    public int VacancyId { get; set; }

    public Vacancy Vacancy { get; set; } = null!;

    public ApplicationType Type { get; set; }

    public ApplicationStatus Status { get; set; }

    public DateTime AppliedAt { get; set; }
}
