namespace BLL.Services;

public sealed class ResumeService : IResumeService
{
    private const int MaxTitleLength = 200;

    private readonly IUnitOfWork _unitOfWork;

    public ResumeService(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ResumeDto>> GetAllAsync(
        ResumeFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var resumes = await _unitOfWork.Resumes
            .GetAllWithUserAsync(cancellationToken)
            .ConfigureAwait(false);

        IEnumerable<Resume> query = ApplyFilter(resumes, filter);

        return [.. query.Select(ResumeMapper.ToDto)];
    }

    public async Task<ResumeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var resume = await _unitOfWork.Resumes
            .GetByIdWithDetailsAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), id);

        return ResumeMapper.ToDto(resume);
    }

    public async Task<IReadOnlyList<ResumeDto>> GetByUserIdAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        var resumes = await _unitOfWork.Resumes
            .GetByUserIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        return [.. resumes.Select(ResumeMapper.ToDto)];
    }

    public async Task<OperationResult<ResumeDto>> CreateAsync(
        CreateResumeDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateCreate(dto);

        var owner = await _unitOfWork.Users
            .GetByIdWithRoleAsync(dto.UserId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), dto.UserId);

        EnsureCanManageResumes(owner);

        var resume = ResumeMapper.ToEntity(dto);
        resume.CreatedAt = resume.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Resumes.AddAsync(resume, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await _unitOfWork.Resumes
            .GetByIdWithDetailsAsync(resume.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), resume.Id);

        return OperationResult.Success(ResumeMapper.ToDto(created));
    }

    public async Task<OperationResult<ResumeDto>> UpdateAsync(
        int id, UpdateResumeDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateUpdate(dto);

        var resume = await _unitOfWork.Resumes
            .GetByIdWithDetailsAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), id);

        ResumeMapper.UpdateEntity(resume, dto);
        resume.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Resumes.Update(resume);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult.Success(ResumeMapper.ToDto(resume));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Resumes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), id);

        await _unitOfWork.Resumes.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<VacancyDto>> FindMatchingVacanciesAsync(
        int resumeId, CancellationToken cancellationToken = default)
    {
        var resume = await _unitOfWork.Resumes
            .GetByIdAsync(resumeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), resumeId);

        var vacancies = await _unitOfWork.Vacancies
            .GetAllWithUserAsync(cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. vacancies
                .Where(v => SkillsMatcher.HasOverlap(resume.Skills, v.RequiredSkills))
                .Select(VacancyMapper.ToDto),
        ];
    }

    private static IEnumerable<Resume> ApplyFilter(
        IReadOnlyList<Resume> source, ResumeFilterDto? filter)
    {
        IEnumerable<Resume> query = source;

        if (filter is null)
            return query.OrderByDescending(r => r.CreatedAt);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.ToUpperInvariant();
            query = query.Where(r =>
                r.Title.ToUpperInvariant().Contains(kw, StringComparison.Ordinal) ||
                r.Description.ToUpperInvariant().Contains(kw, StringComparison.Ordinal) ||
                r.Skills.ToUpperInvariant().Contains(kw, StringComparison.Ordinal));
        }

        if (filter.MinSalary.HasValue)
            query = query.Where(r => r.ExpectedSalary >= filter.MinSalary.Value);

        if (filter.MaxSalary.HasValue)
            query = query.Where(r => r.ExpectedSalary <= filter.MaxSalary.Value);

        return (filter.SortBy?.ToUpperInvariant()) switch
        {
            "TITLE"  => filter.Ascending ? query.OrderBy(r => r.Title)           : query.OrderByDescending(r => r.Title),
            "SALARY" => filter.Ascending ? query.OrderBy(r => r.ExpectedSalary)  : query.OrderByDescending(r => r.ExpectedSalary),
            "DATE"   => filter.Ascending ? query.OrderBy(r => r.CreatedAt)       : query.OrderByDescending(r => r.CreatedAt),
            _        => query.OrderByDescending(r => r.CreatedAt),
        };
    }

    private static void ValidateCreate(CreateResumeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Resume title is required.");
        if (dto.Title.Length > MaxTitleLength)
            throw new ValidationException($"Resume title must not exceed {MaxTitleLength} characters.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ValidationException("Resume description is required.");
        if (string.IsNullOrWhiteSpace(dto.Skills))
            throw new ValidationException("At least one skill is required.");
        if (dto.ExpectedSalary < 0)
            throw new ValidationException("Expected salary must be non-negative.");
    }

    private static void ValidateUpdate(UpdateResumeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Resume title is required.");
        if (dto.Title.Length > MaxTitleLength)
            throw new ValidationException($"Resume title must not exceed {MaxTitleLength} characters.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ValidationException("Resume description is required.");
        if (string.IsNullOrWhiteSpace(dto.Skills))
            throw new ValidationException("At least one skill is required.");
        if (dto.ExpectedSalary < 0)
            throw new ValidationException("Expected salary must be non-negative.");
    }

    private static void EnsureCanManageResumes(User owner)
    {
        var roleName = owner.Role?.Name ?? string.Empty;
        if (roleName != KnownRoles.Employee && roleName != KnownRoles.Administrator)
            throw new AuthorizationException(
                $"Only Employees and Administrators may manage resumes. " +
                $"Current role: '{roleName}'.");
    }
}
