namespace PL.Mappers;

public static class ApplicationMapper
{
    
    public static ApplicationViewModel ToViewModel(ApplicationDto dto) =>
        new(dto.Id,
            dto.ResumeId,
            dto.ResumeTitle,
            dto.VacancyId,
            dto.VacancyTitle,
            dto.Type,
            dto.Status,
            dto.AppliedAt);

    public static ApplyDto ToApplyDto(ApplyRequest request, int userId) =>
        new(request.ResumeId, request.VacancyId, userId);

    public static ProposeDto ToProposeDto(ProposeRequest request, int userId) =>
        new(request.VacancyId, request.ResumeId, userId);
}
