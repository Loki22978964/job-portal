namespace BLL.DTOs;

public sealed record VacancyDto(
    int Id,
    string Title,
    string Description,
    string Company,
    string RequiredSkills,
    decimal Salary,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int UserId,
    string UserFullName);
