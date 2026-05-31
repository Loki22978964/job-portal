namespace BLL.DTOs;

public sealed record CreateVacancyDto(
    string Title,
    string Description,
    string Company,
    
    string RequiredSkills,
    decimal Salary,
    
    int UserId);
