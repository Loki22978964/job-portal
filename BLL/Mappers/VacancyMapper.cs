namespace BLL.Mappers;

public static class VacancyMapper
{
    
    public static VacancyDto ToDto(Vacancy entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var userFullName = entity.User is not null
            ? $"{entity.User.FirstName} {entity.User.LastName}".Trim()
            : string.Empty;

        return new VacancyDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Company,
            entity.RequiredSkills,
            entity.Salary,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.UserId,
            userFullName);
    }

    public static Vacancy ToEntity(CreateVacancyDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Vacancy
        {
            Title = dto.Title,
            Description = dto.Description,
            Company = dto.Company,
            RequiredSkills = dto.RequiredSkills,
            Salary = dto.Salary,
            UserId = dto.UserId,
        };
    }

    public static void UpdateEntity(Vacancy entity, UpdateVacancyDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Company = dto.Company;
        entity.RequiredSkills = dto.RequiredSkills;
        entity.Salary = dto.Salary;
    }
}
