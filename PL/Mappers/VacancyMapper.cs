namespace PL.Mappers;

public static class VacancyMapper
{
    
    public static VacancyViewModel ToViewModel(VacancyDto dto) =>
        new(dto.Id,
            dto.Title,
            dto.Description,
            dto.Company,
            dto.RequiredSkills,
            dto.Salary,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.UserId,
            dto.UserFullName);

    public static CreateVacancyDto ToCreateDto(CreateVacancyRequest request, int userId) =>
        new(request.Title,
            request.Description,
            request.Company,
            request.RequiredSkills,
            request.Salary,
            userId);

    public static UpdateVacancyDto ToUpdateDto(UpdateVacancyRequest request) =>
        new(request.Title,
            request.Description,
            request.Company,
            request.RequiredSkills,
            request.Salary);

    public static VacancyFilterDto ToFilterDto(VacancyFilterRequest request) =>
        new(request.Keyword,
            request.SortBy,
            request.Ascending,
            request.MinSalary,
            request.MaxSalary,
            request.Company);
}
