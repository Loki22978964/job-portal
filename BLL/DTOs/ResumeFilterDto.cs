namespace BLL.DTOs;

public sealed record ResumeFilterDto(
    
    string? Keyword = null,
    
    string? SortBy = null,
    
    bool Ascending = true,
    decimal? MinSalary = null,
    decimal? MaxSalary = null);
