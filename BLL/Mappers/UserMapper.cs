namespace BLL.Mappers;

public static class UserMapper
{
    
    public static UserDto ToDto(User entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserDto(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.Role?.Name ?? string.Empty);
    }
}
