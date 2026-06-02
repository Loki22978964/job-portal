namespace PL.Models;

public sealed record CreateVacancyRequest(
    string Title,
    string Description,
    string Company,
    string RequiredSkills,
    decimal Salary);
