namespace BLL.Interfaces;

public interface IUserService
{
    
    Task<OperationResult<UserDto>> RegisterAsync(
        RegisterDto dto, CancellationToken cancellationToken = default);

    Task<UserDto> AuthenticateAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
}
