namespace PL.Models;

public sealed record ApplicationViewModel(
    int Id,
    int ResumeId,
    string ResumeTitle,
    int VacancyId,
    string VacancyTitle,
    string Type,
    string Status,
    DateTime AppliedAt);
