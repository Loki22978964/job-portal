namespace PL.Models;

public sealed record CreateResumeRequest(
    string Title,
    string Description,
    string Skills,
    decimal ExpectedSalary);
