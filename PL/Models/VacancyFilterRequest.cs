namespace PL.Models;

public sealed class VacancyFilterRequest
{
    
    public string? Keyword { get; set; }

    public string? SortBy { get; set; }

    public bool Ascending { get; set; } = true;

    public decimal? MinSalary { get; set; }

    public decimal? MaxSalary { get; set; }

    public string? Company { get; set; }
}
