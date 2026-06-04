namespace PL.Models;

public sealed record UpdateResumeRequest(
    string Title,
    string Description,
    string Skills,
    decimal ExpectedSalary);
