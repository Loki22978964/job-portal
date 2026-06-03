namespace PL.Models;

public sealed record UserViewModel(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string RoleName);
