namespace BLL.DTOs;

public sealed record PatchResumeDto(
    string? Title = null,
    string? Description = null,
    string? Skills = null,
    decimal? ExpectedSalary = null);
