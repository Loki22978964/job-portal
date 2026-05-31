namespace BLL.DTOs;

public sealed record ApplicationDto(
    int Id,
    int ResumeId,
    string ResumeTitle,
    int VacancyId,
    string VacancyTitle,
    
    string Type,
    
    string Status,
    DateTime AppliedAt);
