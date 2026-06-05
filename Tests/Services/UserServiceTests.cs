using System.Security.Cryptography;
using System.Text;

namespace Tests.Services;

public sealed class UserServiceTests
{
    
    private readonly Mock<IUnitOfWork>         _uow      = new();
    private readonly Mock<IUserRepository>     _userRepo = new();
    private readonly Mock<IRepository<Role>>   _roleRepo = new();
    private readonly UserService               _sut;

    public UserServiceTests()
    {
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);
        _uow.Setup(u => u.Roles).Returns(_roleRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new UserService(_uow.Object);
    }

    private static Role EmployeeRole() => new() { Id = 3, Name = "Employee" };

    private static User MakeUser(int id, string passwordHash) => new()
    {
        Id           = id,
        Email        = "user@test.com",
        PasswordHash = passwordHash,
        FirstName    = "John",
        LastName     = "Doe",
        RoleId       = 3,
        Role         = EmployeeRole(),
    };

    private static RegisterDto ValidRegisterDto(int roleId = 3) =>
        new("new@test.com", "SecurePass1!", "Jane", "Doe", roleId);

    private static string ComputePasswordHash(string password, byte[] salt)
    {
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt, 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    [Fact]
    public async Task RegisterAsync_ValidDto_ReturnsCreatedUserDto()
    {
        
        var dto = ValidRegisterDto();
        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _roleRepo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeRole());
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = 1, Email = dto.Email, FirstName = dto.FirstName,
                LastName = dto.LastName, Role = EmployeeRole(), RoleId = 3,
            });

        var result = await _sut.RegisterAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be(dto.Email);
        result.Data.FirstName.Should().Be(dto.FirstName);
        result.Data.RoleName.Should().Be("Employee");
    }

    [Fact]
    public async Task RegisterAsync_NullDto_ThrowsArgumentNullException()
    {
        
        var act = () => _sut.RegisterAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RegisterAsync_EmptyEmail_ThrowsValidationException()
    {
        
        var dto = new RegisterDto("  ", "Pass1234!", "Jane", "Doe", 3);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public async Task RegisterAsync_EmailWithoutAtSign_ThrowsValidationException()
    {
        
        var dto = new RegisterDto("invalidemail.com", "Pass1234!", "Jane", "Doe", 3);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*valid*");
    }

    [Fact]
    public async Task RegisterAsync_EmailExceedsMaxLength_ThrowsValidationException()
    {
        
        var dto = new RegisterDto($"{new string('a', 251)}@x.com", "Pass1234!", "Jane", "Doe", 3);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*256*");
    }

    [Fact]
    public async Task RegisterAsync_PasswordTooShort_ThrowsValidationException()
    {
        
        var dto = new RegisterDto("x@x.com", "short7!", "Jane", "Doe", 3);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*8*");
    }

    [Fact]
    public async Task RegisterAsync_EmptyFirstName_ThrowsValidationException()
    {
        
        var dto = new RegisterDto("x@x.com", "ValidPass1!", "  ", "Doe", 3);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*First name*");
    }

    [Fact]
    public async Task RegisterAsync_EmptyLastName_ThrowsValidationException()
    {
        
        var dto = new RegisterDto("x@x.com", "ValidPass1!", "Jane", "  ", 3);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Last name*");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsDuplicateEntityException()
    {
        
        var dto = ValidRegisterDto();
        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 5, Email = dto.Email }); 

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage($"*{dto.Email}*");
    }

    [Fact]
    public async Task RegisterAsync_RoleNotFound_ThrowsEntityNotFoundException()
    {
        
        var dto = new RegisterDto("x@x.com", "ValidPass1!", "Jane", "Doe", 999);
        _userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _roleRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsUserDto()
    {
        
        const string password = "ValidPass1!";
        byte[] salt = new byte[16];
        salt[0] = 0x42; 
        var hash = ComputePasswordHash(password, salt);
        var user = MakeUser(1, hash);

        _userRepo.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.AuthenticateAsync(new LoginDto("user@test.com", password));

        result.Email.Should().Be("user@test.com");
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task AuthenticateAsync_EmailNotFound_ThrowsAuthorizationException()
    {
        
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.AuthenticateAsync(new LoginDto("ghost@test.com", "anyPassword1!"));

        await act.Should().ThrowAsync<AuthorizationException>()
            .WithMessage("*Invalid*");
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ThrowsAuthorizationException()
    {
        
        byte[] salt = new byte[16];
        salt[0] = 0x99;
        var hash = ComputePasswordHash("CorrectPass1!", salt);
        var user = MakeUser(1, hash);

        _userRepo.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _sut.AuthenticateAsync(new LoginDto("user@test.com", "WrongPassword!"));

        await act.Should().ThrowAsync<AuthorizationException>()
            .WithMessage("*Invalid*");
    }

    [Fact]
    public async Task AssignRoleAsync_ValidUserAndRole_UpdatesUserRoleAndSaves()
    {
        
        var user    = new User { Id = 5, Email = "x@x.com", RoleId = 3 };
        var newRole = new Role { Id = 2, Name = "Employer" };

        _userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRole);

        await _sut.AssignRoleAsync(5, 2);

        user.RoleId.Should().Be(2);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_UserNotFound_ThrowsEntityNotFoundException()
    {
        
        _userRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.AssignRoleAsync(99, 2);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AssignRoleAsync_RoleNotFound_ThrowsEntityNotFoundException()
    {
        
        _userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 5 });
        _roleRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var act = () => _sut.AssignRoleAsync(5, 99);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
