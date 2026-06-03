namespace PL.Models;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    int RoleId);
