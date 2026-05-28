namespace BLL.DTOs;

public sealed record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    
    int RoleId);
