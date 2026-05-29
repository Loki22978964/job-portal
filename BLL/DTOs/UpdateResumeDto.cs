namespace BLL.DTOs;

public sealed record UpdateResumeDto(
    string Title,
    string Description,
    string Skills,
    decimal ExpectedSalary);
