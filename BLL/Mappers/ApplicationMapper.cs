namespace BLL.Mappers;

public static class ApplicationMapper
{
    
    public static ApplicationDto ToDto(Application entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ApplicationDto(
            entity.Id,
            entity.ResumeId,
            entity.Resume?.Title ?? string.Empty,
            entity.VacancyId,
            entity.Vacancy?.Title ?? string.Empty,
            entity.Type.ToString(),
            entity.Status.ToString(),
            entity.AppliedAt);
    }
}
