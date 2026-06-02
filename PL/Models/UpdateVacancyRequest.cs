namespace PL.Models;

public sealed record UpdateVacancyRequest(
    string Title,
    string Description,
    string Company,
    string RequiredSkills,
    decimal Salary);
