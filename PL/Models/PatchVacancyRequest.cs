namespace PL.Models;

public sealed record PatchVacancyRequest(
    string? Title,
    string? Description,
    string? Company,
    string? RequiredSkills,
    decimal? Salary);
