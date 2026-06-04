namespace PL.Models;

public sealed record ResumeViewModel(
    int Id,
    string Title,
    string Description,
    string Skills,
    decimal ExpectedSalary,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int UserId,
    string UserFullName);
