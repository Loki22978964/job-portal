namespace BLL.DTOs;

public sealed record UpdateVacancyDto(
    string Title,
    string Description,
    string Company,
    string RequiredSkills,
    decimal Salary);
