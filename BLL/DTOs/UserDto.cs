namespace BLL.DTOs;

public sealed record UserDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string RoleName);
