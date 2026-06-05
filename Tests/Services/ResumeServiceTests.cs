namespace Tests.Services;

public sealed class ResumeServiceTests
{
    
    private readonly Mock<IUnitOfWork>         _uow         = new();
    private readonly Mock<IResumeRepository>   _resumeRepo  = new();
    private readonly Mock<IUserRepository>     _userRepo    = new();
    private readonly Mock<IVacancyRepository>  _vacancyRepo = new();
    private readonly ResumeService             _sut;

    public ResumeServiceTests()
    {
        _uow.Setup(u => u.Resumes).Returns(_resumeRepo.Object);
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);
        _uow.Setup(u => u.Vacancies).Returns(_vacancyRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _resumeRepo.Setup(r => r.AddAsync(It.IsAny<Resume>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _resumeRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new ResumeService(_uow.Object);
    }

    private static Role EmployeeRole() => new() { Id = 3, Name = "Employee" };
    private static Role EmployerRole() => new() { Id = 2, Name = "Employer" };
    private static Role AdminRole()    => new() { Id = 1, Name = "Administrator" };

    private static User EmployeeUser() => new()
    {
        Id = 20, FirstName = "Bob", LastName = "Jones",
        Email = "bob@email.com", RoleId = 3, Role = EmployeeRole(),
    };

    private static User EmployerUser() => new()
    {
        Id = 10, FirstName = "Alice", LastName = "Smith",
        Email = "alice@corp.com", RoleId = 2, Role = EmployerRole(),
    };

    private static User AdminUser() => new()
    {
        Id = 30, FirstName = "Admin", LastName = "Root",
        Email = "admin@corp.com", RoleId = 1, Role = AdminRole(),
    };

    private static Resume MakeResume(int id, string skills = "C#,SQL") => new()
    {
        Id             = id,
        Title          = $"Resume {id}",
        Description    = "Experienced developer",
        Skills         = skills,
        ExpectedSalary = 3_000m,
        CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(id - 1),
        UpdatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(id - 1),
        UserId         = 20,
        User           = EmployeeUser(),
    };

    private static Vacancy MakeVacancy(int id, string skills = "C#,SQL") => new()
    {
        Id             = id,
        Title          = $"Vacancy {id}",
        Description    = "Job description",
        Company        = "Corp",
        RequiredSkills = skills,
        Salary         = 4_000m,
        CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UserId         = 10,
        User           = EmployerUser(),
    };

    private static CreateResumeDto ValidCreateDto(int userId = 20) =>
        new("Senior C# Dev", "5 years experience", "C#,SQL,Docker", 5_000m, userId);

    [Fact]
    public async Task CreateAsync_UserHasEmployeeRole_ReturnsSuccessResult()
    {
        
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeUser());
        _resumeRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1));

        var result = await _sut.CreateAsync(ValidCreateDto(userId: 20));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_UserHasAdministratorRole_ReturnsSuccessResult()
    {
        
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AdminUser());
        _resumeRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1));

        var result = await _sut.CreateAsync(ValidCreateDto(userId: 30));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_UserHasEmployerRole_ThrowsAuthorizationException()
    {
        
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployerUser());

        var act = () => _sut.CreateAsync(ValidCreateDto(userId: 10));

        await act.Should().ThrowAsync<AuthorizationException>()
            .WithMessage("*Employees and Administrators*");
    }

    [Fact]
    public async Task FindMatchingVacanciesAsync_VacancyRequiredSkillsOverlap_ReturnsMatchingVacancies()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1, skills: "C#,Docker"));

        var vacancies = new List<Vacancy>
        {
            MakeVacancy(1, skills: "C#,SQL"),         
            MakeVacancy(2, skills: "Docker,K8s"),     
            MakeVacancy(3, skills: "Java,Kotlin"),    
        };
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancies);

        var result = await _sut.FindMatchingVacanciesAsync(1);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.Id == 1 || v.Id == 2);
    }

    [Fact]
    public async Task FindMatchingVacanciesAsync_NoSkillOverlap_ReturnsEmptyList()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1, skills: "Erlang,Elixir"));
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { MakeVacancy(1, skills: "C#,SQL") });

        var result = await _sut.FindMatchingVacanciesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindMatchingVacanciesAsync_ResumeNotFound_ThrowsEntityNotFoundException()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resume?)null);

        var act = () => _sut.FindMatchingVacanciesAsync(99);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
