namespace PL.Models;

public sealed record VacancyViewModel(
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
