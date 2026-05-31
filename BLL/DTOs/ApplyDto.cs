namespace BLL.DTOs;

public sealed record ApplyDto(
    
    int ResumeId,
    
    int VacancyId,
    
    int ApplicantUserId);
