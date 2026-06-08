namespace Tests.Services;

public sealed class ApplicationServiceTests
{
    
    private static readonly int[] s_linkedVacancyIds = [2, 3];
    private static readonly int[] s_linkedResumeIds  = [1, 4];

    private readonly Mock<IUnitOfWork>            _uow             = new();
    private readonly Mock<IResumeRepository>      _resumeRepo      = new();
    private readonly Mock<IVacancyRepository>     _vacancyRepo     = new();
    private readonly Mock<IApplicationRepository> _applicationRepo = new();
    private readonly ApplicationService           _sut;

    public ApplicationServiceTests()
    {
        _uow.Setup(u => u.Resumes).Returns(_resumeRepo.Object);
        _uow.Setup(u => u.Vacancies).Returns(_vacancyRepo.Object);
        _uow.Setup(u => u.Applications).Returns(_applicationRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _applicationRepo.Setup(r => r.AddAsync(It.IsAny<Application>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new ApplicationService(_uow.Object);
    }

    private static User SampleUser(int id) => new()
    {
        Id = id, FirstName = $"User{id}", LastName = "Test", Email = $"user{id}@test.com",
        Role = new Role { Id = 3, Name = "Employee" }, RoleId = 3,
    };

    private static Resume MakeResume(int id, int userId = 10) => new()
    {
        Id = id, Title = $"Resume {id}", Description = "Desc",
        Skills = "C#", ExpectedSalary = 3_000m,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        UserId = userId, User = SampleUser(userId),
    };

    private static Vacancy MakeVacancy(int id, int userId = 20) => new()
    {
        Id = id, Title = $"Vacancy {id}", Description = "Desc",
        Company = "Corp", RequiredSkills = "C#", Salary = 4_000m,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        UserId = userId, User = SampleUser(userId),
    };

    private static Application MakeApplication(int id, int resumeId, int vacancyId,
        ApplicationType type = ApplicationType.Apply) => new()
    {
        Id        = id,
        ResumeId  = resumeId,
        VacancyId = vacancyId,
        Type      = type,
        Status    = ApplicationStatus.Pending,
        AppliedAt = DateTime.UtcNow,
        Resume    = MakeResume(resumeId),
        Vacancy   = MakeVacancy(vacancyId),
    };

    [Fact]
    public async Task ApplyAsync_ValidRequest_CreatesApplicationAndReturnsDto()
    {
        
        var resume  = MakeResume(1, userId: 10);
        var vacancy = MakeVacancy(2, userId: 20);
        var created = MakeApplication(99, resumeId: 1, vacancyId: 2, type: ApplicationType.Apply);

        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resume);
        _vacancyRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vacancy);
        _applicationRepo.Setup(r => r.ExistsAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _applicationRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var dto = new ApplyDto(ResumeId: 1, VacancyId: 2, ApplicantUserId: 10);

        var result = await _sut.ApplyAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Type.Should().Be(ApplicationType.Apply.ToString());
        result.Data.Status.Should().Be(ApplicationStatus.Pending.ToString());
    }

    [Fact]
    public async Task ApplyAsync_NullDto_ThrowsArgumentNullException()
    {
        
        var act = () => _sut.ApplyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ApplyAsync_ResumeNotFound_ThrowsEntityNotFoundException()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resume?)null);

        var dto = new ApplyDto(ResumeId: 99, VacancyId: 1, ApplicantUserId: 10);

        var act = () => _sut.ApplyAsync(dto);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ApplyAsync_ResumeBelongsToDifferentUser_ThrowsAuthorizationException()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1, userId: 10));

        var dto = new ApplyDto(ResumeId: 1, VacancyId: 2, ApplicantUserId: 99);

        var act = () => _sut.ApplyAsync(dto);

        await act.Should().ThrowAsync<AuthorizationException>()
            .WithMessage("*belongs to your own account*");
    }

    [Fact]
    public async Task ApplyAsync_VacancyNotFound_ThrowsEntityNotFoundException()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1, userId: 10));
        _vacancyRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vacancy?)null);

        var dto = new ApplyDto(ResumeId: 1, VacancyId: 99, ApplicantUserId: 10);

        var act = () => _sut.ApplyAsync(dto);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ApplyAsync_DuplicateApplication_ThrowsDuplicateEntityException()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1, userId: 10));
        _vacancyRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVacancy(2));
        _applicationRepo.Setup(r => r.ExistsAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); 

        var dto = new ApplyDto(ResumeId: 1, VacancyId: 2, ApplicantUserId: 10);

        var act = () => _sut.ApplyAsync(dto);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task GetLinkedVacanciesAsync_ResumeNotFound_ThrowsEntityNotFoundException()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resume?)null);

        var act = () => _sut.GetLinkedVacanciesAsync(99);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetLinkedVacanciesAsync_ExistingResume_ReturnsVacanciesFromApplications()
    {
        
        _resumeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeResume(1));

        var applications = new List<Application>
        {
            new() { Id = 10, ResumeId = 1, VacancyId = 2, Vacancy = MakeVacancy(2) },
            new() { Id = 11, ResumeId = 1, VacancyId = 3, Vacancy = MakeVacancy(3) },
        };
        _applicationRepo.Setup(r => r.GetByResumeIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _sut.GetLinkedVacanciesAsync(1);

        result.Should().HaveCount(2);
        result.Select(v => v.Id).Should().BeEquivalentTo(s_linkedVacancyIds);
    }

    [Fact]
    public async Task GetLinkedResumesAsync_VacancyNotFound_ThrowsEntityNotFoundException()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vacancy?)null);

        var act = () => _sut.GetLinkedResumesAsync(99);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetLinkedResumesAsync_ExistingVacancy_ReturnsResumesFromApplications()
    {
        
        _vacancyRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeVacancy(2));

        var applications = new List<Application>
        {
            new() { Id = 10, ResumeId = 1, VacancyId = 2, Resume = MakeResume(1) },
            new() { Id = 11, ResumeId = 4, VacancyId = 2, Resume = MakeResume(4) },
        };
        _applicationRepo.Setup(r => r.GetByVacancyIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _sut.GetLinkedResumesAsync(2);

        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().BeEquivalentTo(s_linkedResumeIds);
    }
}
