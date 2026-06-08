namespace BLL.DTOs;

public sealed record PatchVacancyDto(
    string? Title = null,
    string? Description = null,
    string? Company = null,
    string? RequiredSkills = null,
    decimal? Salary = null);
