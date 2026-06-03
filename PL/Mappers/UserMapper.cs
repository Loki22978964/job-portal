namespace PL.Mappers;

public static class UserMapper
{
    
    public static UserViewModel ToViewModel(UserDto dto) =>
        new(dto.Id, dto.Email, dto.FirstName, dto.LastName, dto.RoleName);

    public static RegisterDto ToRegisterDto(RegisterRequest request) =>
        new(request.Email, request.Password, request.FirstName, request.LastName, request.RoleId);

    public static LoginDto ToLoginDto(LoginRequest request) =>
        new(request.Email, request.Password);
}
