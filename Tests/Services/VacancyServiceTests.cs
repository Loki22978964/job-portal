namespace Tests.Services;

public sealed class VacancyServiceTests
{
    
    private readonly Mock<IUnitOfWork>            _uow             = new();
    private readonly Mock<IVacancyRepository>     _vacancyRepo     = new();
    private readonly Mock<IUserRepository>        _userRepo        = new();
    private readonly Mock<IApplicationRepository> _applicationRepo = new();
    private readonly Mock<IResumeRepository>      _resumeRepo      = new();
    private readonly VacancyService               _sut;

    public VacancyServiceTests()
    {
        _uow.Setup(u => u.Vacancies).Returns(_vacancyRepo.Object);
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);
        _uow.Setup(u => u.Applications).Returns(_applicationRepo.Object);
        _uow.Setup(u => u.Resumes).Returns(_resumeRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _vacancyRepo.Setup(r => r.AddAsync(It.IsAny<Vacancy>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _vacancyRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _applicationRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new VacancyService(_uow.Object);
    }

    private static Role EmployerRole()  => new() { Id = 2, Name = "Employer" };
    private static Role EmployeeRole()  => new() { Id = 3, Name = "Employee" };
    private static Role AdminRole()     => new() { Id = 1, Name = "Administrator" };

    private static User EmployerUser() => new()
    {
        Id = 10, FirstName = "Alice", LastName = "Smith",
        Email = "alice@corp.com", RoleId = 2, Role = EmployerRole(),
    };

    private static User EmployeeUser() => new()
    {
        Id = 20, FirstName = "Bob", LastName = "Jones",
        Email = "bob@email.com", RoleId = 3, Role = EmployeeRole(),
    };

    private static User AdminUser() => new()
    {
        Id = 30, FirstName = "Admin", LastName = "Root",
        Email = "admin@corp.com", RoleId = 1, Role = AdminRole(),
    };

    private static Vacancy MakeVacancy(
        int     id,
        string  skills  = "C#,SQL",
        decimal salary  = 3_000m,
        string? title   = null,
        string? company = null,
        DateTime? created = null) => new()
    {
        Id             = id,
        Title          = title   ?? $"Dev Role {id}",
        Description    = "Full-stack developer position",
        Company        = company ?? "Tech Corp",
        RequiredSkills = skills,
        Salary         = salary,
        CreatedAt      = created ?? new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(id - 1),
        UpdatedAt      = created ?? new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(id - 1),
        UserId         = 10,
        User           = EmployerUser(),
    };

    private static CreateVacancyDto ValidCreateDto(int userId = 10) =>
        new("Senior Backend Dev", "Build scalable APIs", "Tech Corp", "C#,SQL,Docker", 6_000m, userId);

    private static UpdateVacancyDto ValidUpdateDto() =>
        new("Updated Role", "New description", "New Corp", "Java,Spring", 7_000m);

    [Fact]
    public async Task GetAllAsync_NullFilter_ReturnsAllVacanciesOrderedByDateDescending()
    {
        
        var oldest  = MakeVacancy(1, created: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newest  = MakeVacancy(2, created: new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        var middle  = MakeVacancy(3, created: new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { oldest, newest, middle });

        var result = await _sut.GetAllAsync();

        result.Select(v => v.Id).Should().Equal(2, 3, 1);
    }

    [Fact]
    public async Task GetAllAsync_KeywordMatchesTitle_ReturnsOnlyMatchingVacancies()
    {
        
        var match   = MakeVacancy(1, title: "C# Backend Developer");
        var noMatch = MakeVacancy(2, title: "Java Frontend Engineer");
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { match, noMatch });

        var result = await _sut.GetAllAsync(new VacancyFilterDto(Keyword: "backend"));

        result.Should().ContainSingle(v => v.Id == 1);
    }

    [Fact]
    public async Task GetAllAsync_CompanyFilter_ReturnsOnlyVacanciesMatchingCompany()
    {
        
        var alpha1  = MakeVacancy(1, company: "Alpha Corp");
        var beta    = MakeVacancy(2, company: "Beta Inc");
        var alpha2  = MakeVacancy(3, company: "Alpha Solutions");
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { alpha1, beta, alpha2 });

        var result = await _sut.GetAllAsync(new VacancyFilterDto(Company: "alpha"));

        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.Id == 1 || v.Id == 3);
    }

    [Fact]
    public async Task GetAllAsync_SalaryRangeFilter_ReturnsVacanciesWithinRange()
    {
        
        var low  = MakeVacancy(1, salary: 1_000m);
        var mid  = MakeVacancy(2, salary: 3_000m);
        var high = MakeVacancy(3, salary: 5_000m);
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { low, mid, high });

        var result = await _sut.GetAllAsync(new VacancyFilterDto(MinSalary: 2_000m, MaxSalary: 4_000m));

        result.Should().ContainSingle(v => v.Id == 2);
    }

    [Fact]
    public async Task GetAllAsync_SortByTitleAscending_ReturnsTitlesInAlphabeticalOrder()
    {
        
        var c  = MakeVacancy(1, title: "C# Developer");
        var a  = MakeVacancy(2, title: "Architect");
        var b  = MakeVacancy(3, title: "Backend Lead");
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { c, a, b });

        var result = await _sut.GetAllAsync(new VacancyFilterDto(SortBy: "title", Ascending: true));

        result.Select(v => v.Title).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SortBySalaryDescending_ReturnsHighestSalaryFirst()
    {
        
        var low  = MakeVacancy(1, salary: 2_000m);
        var high = MakeVacancy(2, salary: 5_000m);
        var mid  = MakeVacancy(3, salary: 3_500m);
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { low, high, mid });

        var result = await _sut.GetAllAsync(new VacancyFilterDto(SortBy: "SALARY", Ascending: false));

        result.Select(v => v.Salary).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_SortByCompanyAscending_ReturnsSortedByCompanyName()
    {
        
        var z = MakeVacancy(1, company: "Zebra Inc");
        var a = MakeVacancy(2, company: "Alpha Corp");
        _vacancyRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vacancy> { z, a });

        var result = await _sut.GetAllAsync(new VacancyFilterDto(SortBy: "company", Ascending: true));

        result.Select(v => v.Company).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectVacancyDto()
    {
        
        var vacancy = MakeVacancy(42);
        _vacancyRepo.Setup(r => r.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy);

        var result = await _sut.GetByIdAsync(42);

        result.Id.Should().Be(42);
        result.Title.Should().Be(vacancy.Title);
        result.UserFullName.Should().Be("Alice Smith");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsEntityNotFoundException()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdWithDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vacancy?)null);

        var act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ReturnsOnlyThatUsersVacancies()
    {
        
        var userVacancies = new List<Vacancy> { MakeVacancy(1), MakeVacancy(2) };
        _vacancyRepo.Setup(r => r.GetByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userVacancies);

        var result = await _sut.GetByUserIdAsync(10);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.UserId == 10);
    }

    [Fact]
    public async Task CreateAsync_ValidDtoWithEmployerRole_ReturnsSuccessResult()
    {
        
        var dto = ValidCreateDto(userId: 10);
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployerUser());
        _vacancyRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVacancy(1));

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDtoWithAdministratorRole_ReturnsSuccessResult()
    {
        
        var dto = ValidCreateDto(userId: 30);
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AdminUser());
        _vacancyRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVacancy(1));

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
    {
        
        var act = () => _sut.CreateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto("   ", "Desc", "Corp", "C#", 1_000m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*title*");
    }

    [Fact]
    public async Task CreateAsync_TitleExceedsMaxLength_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto(new string('A', 201), "Desc", "Corp", "C#", 1_000m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*200*");
    }

    [Fact]
    public async Task CreateAsync_EmptyDescription_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto("Valid Title", "  ", "Corp", "C#", 1_000m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*description*");
    }

    [Fact]
    public async Task CreateAsync_EmptyCompany_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto("Valid Title", "Valid Desc", "  ", "C#", 1_000m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Company*");
    }

    [Fact]
    public async Task CreateAsync_CompanyExceedsMaxLength_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto("Title", "Desc", new string('B', 201), "C#", 1_000m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*200*");
    }

    [Fact]
    public async Task CreateAsync_EmptyRequiredSkills_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto("Title", "Desc", "Corp", "   ", 1_000m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*skill*");
    }

    [Fact]
    public async Task CreateAsync_NegativeSalary_ThrowsValidationException()
    {
        
        var dto = new CreateVacancyDto("Title", "Desc", "Corp", "C#", -1m, 10);

        var act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*non-negative*");
    }

    [Fact]
    public async Task CreateAsync_UserNotFound_ThrowsEntityNotFoundException()
    {
        
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.CreateAsync(ValidCreateDto(userId: 99));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_UserHasEmployeeRole_ThrowsAuthorizationException()
    {
        
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmployeeUser());

        var act = () => _sut.CreateAsync(ValidCreateDto(userId: 20));

        await act.Should().ThrowAsync<AuthorizationException>()
            .WithMessage("*Employers and Administrators*");
    }

    [Fact]
    public async Task UpdateAsync_ExistingVacancy_ReturnsUpdatedFields()
    {
        
        var vacancy = MakeVacancy(5);
        _vacancyRepo.Setup(r => r.GetByIdWithDetailsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy);
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(vacancy.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy.User);
        
        var dto = ValidUpdateDto();

        var result = await _sut.UpdateAsync(5, dto, vacancy.UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be(dto.Title);
        result.Data.Salary.Should().Be(dto.Salary);
    }

    [Fact]
    public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
    {
        
        var act = () => _sut.UpdateAsync(1, null!, 10);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_VacancyNotFound_ThrowsEntityNotFoundException()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdWithDetailsAsync(77, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vacancy?)null);

        var act = () => _sut.UpdateAsync(77, ValidUpdateDto(), 10);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_EmptyTitle_ThrowsValidationException()
    {
        
        var dto = new UpdateVacancyDto("", "Desc", "Corp", "C#", 1_000m);

        var act = () => _sut.UpdateAsync(1, dto, 10);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*title*");
    }

    [Fact]
    public async Task UpdateAsync_NegativeSalary_ThrowsValidationException()
    {
        
        var dto = new UpdateVacancyDto("Title", "Desc", "Corp", "C#", -100m);

        var act = () => _sut.UpdateAsync(1, dto, 10);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*non-negative*");
    }

    [Fact]
    public async Task DeleteAsync_ExistingVacancyWithNoApplications_DeletesVacancy()
    {
        
        var vacancy = MakeVacancy(5);
        _vacancyRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy);
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(vacancy.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy.User);
        _applicationRepo.Setup(r => r.GetByVacancyIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Application>());

        await _sut.DeleteAsync(5, vacancy.UserId);

        _vacancyRepo.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_VacancyHasApplications_DeletesApplicationsBeforeVacancy()
    {
        
        var applications = new List<Application>
        {
            new() { Id = 101, VacancyId = 5, ResumeId = 1 },
            new() { Id = 102, VacancyId = 5, ResumeId = 2 },
        };
        var vacancy = MakeVacancy(5);
        _vacancyRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy);
        _userRepo.Setup(r => r.GetByIdWithRoleAsync(vacancy.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy.User);
        _applicationRepo.Setup(r => r.GetByVacancyIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        await _sut.DeleteAsync(5, vacancy.UserId);

        _applicationRepo.Verify(r => r.DeleteAsync(101, It.IsAny<CancellationToken>()), Times.Once);
        _applicationRepo.Verify(r => r.DeleteAsync(102, It.IsAny<CancellationToken>()), Times.Once);
        _vacancyRepo.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsEntityNotFoundException()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vacancy?)null);

        var act = () => _sut.DeleteAsync(99, 10);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task FindMatchingResumesAsync_ResumeSkillsOverlap_ReturnsMatchingResumes()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVacancy(1, skills: "C#,SQL"));

        var resumes = new List<Resume>
        {
            new() { Id = 1, Title = "Backend Dev",  Skills = "C#,Java",   UserId = 20, User = EmployeeUser() },
            new() { Id = 2, Title = "DBA Expert",   Skills = "SQL,NoSQL", UserId = 21, User = EmployeeUser() },
            new() { Id = 3, Title = "Go Developer", Skills = "Go,Python", UserId = 22, User = EmployeeUser() },
        };
        _resumeRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resumes);

        var result = await _sut.FindMatchingResumesAsync(1);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Id == 1 || r.Id == 2);
    }

    [Fact]
    public async Task FindMatchingResumesAsync_NoSkillOverlap_ReturnsEmptyList()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVacancy(1, skills: "Rust,WASM"));
        _resumeRepo.Setup(r => r.GetAllWithUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Resume>
            {
                new() { Id = 1, Skills = "C#,SQL", UserId = 20, User = EmployeeUser() },
            });

        var result = await _sut.FindMatchingResumesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindMatchingResumesAsync_VacancyNotFound_ThrowsEntityNotFoundException()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vacancy?)null);

        var act = () => _sut.FindMatchingResumesAsync(99);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
