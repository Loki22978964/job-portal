namespace BLL.Mappers;

public static class ResumeMapper
{
    
    public static ResumeDto ToDto(Resume entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var userFullName = entity.User is not null
            ? $"{entity.User.FirstName} {entity.User.LastName}".Trim()
            : string.Empty;

        return new ResumeDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Skills,
            entity.ExpectedSalary,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.UserId,
            userFullName);
    }

    public static Resume ToEntity(CreateResumeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Resume
        {
            Title = dto.Title,
            Description = dto.Description,
            Skills = dto.Skills,
            ExpectedSalary = dto.ExpectedSalary,
            UserId = dto.UserId,
        };
    }

    public static void UpdateEntity(Resume entity, UpdateResumeDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Skills = dto.Skills;
        entity.ExpectedSalary = dto.ExpectedSalary;
    }
}
