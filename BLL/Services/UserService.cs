namespace BLL.Services;

public sealed class UserService : IUserService
{
    private const int MinPasswordLength = 8;
    private const int MaxEmailLength    = 256;
    private const int MaxNameLength     = 100;

    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<UserDto>> RegisterAsync(
        RegisterDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateRegister(dto);

        var existing = await _unitOfWork.Users
            .GetByEmailAsync(dto.Email, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
            throw new DuplicateEntityException($"A user with email '{dto.Email}' already exists.");

        _ = await _unitOfWork.Roles
            .GetByIdAsync(dto.RoleId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException("Role", dto.RoleId);

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await _unitOfWork.Users
            .GetByIdWithRoleAsync(user.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), user.Id);

        return OperationResult.Success(UserMapper.ToDto(created));
    }

    public async Task<UserDto> AuthenticateAsync(
        LoginDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await _unitOfWork.Users
            .GetByEmailAsync(dto.Email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            throw new AuthorizationException("Invalid email or password.");

        return UserMapper.ToDto(user);
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users
            .GetByIdWithRoleAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), id);

        return UserMapper.ToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. users.Select(UserMapper.ToDto)];
    }

    public async Task AssignRoleAsync(
        int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users
            .GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), userId);

        _ = await _unitOfWork.Roles
            .GetByIdAsync(roleId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException("Role", roleId);

        user.RoleId = roleId;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateRegister(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Email is required.");
        if (!dto.Email.Contains('@', StringComparison.Ordinal))
            throw new ValidationException("Email must be a valid address.");
        if (dto.Email.Length > MaxEmailLength)
            throw new ValidationException($"Email must not exceed {MaxEmailLength} characters.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("Password is required.");
        if (dto.Password.Length < MinPasswordLength)
            throw new ValidationException($"Password must be at least {MinPasswordLength} characters.");

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ValidationException("First name is required.");
        if (dto.FirstName.Length > MaxNameLength)
            throw new ValidationException($"First name must not exceed {MaxNameLength} characters.");

        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new ValidationException("Last name is required.");
        if (dto.LastName.Length > MaxNameLength)
            throw new ValidationException($"Last name must not exceed {MaxNameLength} characters.");
    }
}
