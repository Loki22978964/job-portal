namespace BLL.DTOs;

public sealed record ResumeDto(
    int Id,
    string Title,
    string Description,
    string Skills,
    decimal ExpectedSalary,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int UserId,
    string UserFullName);
