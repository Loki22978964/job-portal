namespace BLL.DTOs;

public sealed record VacancyFilterDto(
    
    string? Keyword = null,
    
    string? SortBy = null,
    bool Ascending = true,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    
    string? Company = null);
