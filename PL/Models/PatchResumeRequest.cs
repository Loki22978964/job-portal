namespace PL.Models;

public sealed record PatchResumeRequest(
    string? Title,
    string? Description,
    string? Skills,
    decimal? ExpectedSalary);
