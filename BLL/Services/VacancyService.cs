namespace BLL.Services;

public sealed class VacancyService : IVacancyService
{
    private const int MaxTitleLength   = 200;
    private const int MaxCompanyLength = 200;

    private readonly IUnitOfWork _unitOfWork;

    public VacancyService(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VacancyDto>> GetAllAsync(
        VacancyFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var vacancies = await _unitOfWork.Vacancies
            .GetAllWithUserAsync(cancellationToken)
            .ConfigureAwait(false);

        IEnumerable<Vacancy> query = ApplyFilter(vacancies, filter);

        return [.. query.Select(VacancyMapper.ToDto)];
    }

    public async Task<VacancyDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vacancy = await _unitOfWork.Vacancies
            .GetByIdWithDetailsAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), id);

        return VacancyMapper.ToDto(vacancy);
    }

    public async Task<IReadOnlyList<VacancyDto>> GetByUserIdAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        var vacancies = await _unitOfWork.Vacancies
            .GetByUserIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        return [.. vacancies.Select(VacancyMapper.ToDto)];
    }

    public async Task<OperationResult<VacancyDto>> CreateAsync(
        CreateVacancyDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateCreate(dto);

        var owner = await _unitOfWork.Users
            .GetByIdWithRoleAsync(dto.UserId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), dto.UserId);

        EnsureCanManageVacancies(owner);

        var vacancy = VacancyMapper.ToEntity(dto);
        vacancy.CreatedAt = vacancy.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Vacancies.AddAsync(vacancy, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await _unitOfWork.Vacancies
            .GetByIdWithDetailsAsync(vacancy.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), vacancy.Id);

        return OperationResult.Success(VacancyMapper.ToDto(created));
    }

    public async Task<OperationResult<VacancyDto>> UpdateAsync(
        int id, UpdateVacancyDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateUpdate(dto);

        var vacancy = await _unitOfWork.Vacancies
            .GetByIdWithDetailsAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), id);

        VacancyMapper.UpdateEntity(vacancy, dto);
        vacancy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Vacancies.Update(vacancy);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult.Success(VacancyMapper.ToDto(vacancy));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Vacancies
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), id);

        var applications = await _unitOfWork.Applications
            .GetByVacancyIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        foreach (var application in applications)
            await _unitOfWork.Applications
                .DeleteAsync(application.Id, cancellationToken)
                .ConfigureAwait(false);

        await _unitOfWork.Vacancies.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ResumeDto>> FindMatchingResumesAsync(
        int vacancyId, CancellationToken cancellationToken = default)
    {
        var vacancy = await _unitOfWork.Vacancies
            .GetByIdAsync(vacancyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), vacancyId);

        var resumes = await _unitOfWork.Resumes
            .GetAllWithUserAsync(cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. resumes
                .Where(r => SkillsMatcher.HasOverlap(vacancy.RequiredSkills, r.Skills))
                .Select(ResumeMapper.ToDto),
        ];
    }

    private static IEnumerable<Vacancy> ApplyFilter(
        IReadOnlyList<Vacancy> source, VacancyFilterDto? filter)
    {
        IEnumerable<Vacancy> query = source;

        if (filter is null)
            return query.OrderByDescending(v => v.CreatedAt);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.ToUpperInvariant();
            query = query.Where(v =>
                v.Title.ToUpperInvariant().Contains(kw, StringComparison.Ordinal) ||
                v.Description.ToUpperInvariant().Contains(kw, StringComparison.Ordinal) ||
                v.RequiredSkills.ToUpperInvariant().Contains(kw, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(filter.Company))
        {
            var company = filter.Company.ToUpperInvariant();
            query = query.Where(v => v.Company.ToUpperInvariant().Contains(company, StringComparison.Ordinal));
        }

        if (filter.MinSalary.HasValue)
            query = query.Where(v => v.Salary >= filter.MinSalary.Value);

        if (filter.MaxSalary.HasValue)
            query = query.Where(v => v.Salary <= filter.MaxSalary.Value);

        return (filter.SortBy?.ToUpperInvariant()) switch
        {
            "TITLE"   => filter.Ascending ? query.OrderBy(v => v.Title)   : query.OrderByDescending(v => v.Title),
            "SALARY"  => filter.Ascending ? query.OrderBy(v => v.Salary)  : query.OrderByDescending(v => v.Salary),
            "COMPANY" => filter.Ascending ? query.OrderBy(v => v.Company) : query.OrderByDescending(v => v.Company),
            "DATE"    => filter.Ascending ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt),
            _         => query.OrderByDescending(v => v.CreatedAt),
        };
    }

    private static void ValidateCreate(CreateVacancyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Vacancy title is required.");
        if (dto.Title.Length > MaxTitleLength)
            throw new ValidationException($"Vacancy title must not exceed {MaxTitleLength} characters.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ValidationException("Vacancy description is required.");
        if (string.IsNullOrWhiteSpace(dto.Company))
            throw new ValidationException("Company name is required.");
        if (dto.Company.Length > MaxCompanyLength)
            throw new ValidationException($"Company name must not exceed {MaxCompanyLength} characters.");
        if (string.IsNullOrWhiteSpace(dto.RequiredSkills))
            throw new ValidationException("At least one required skill must be specified.");
        if (dto.Salary < 0)
            throw new ValidationException("Salary must be non-negative.");
    }

    private static void ValidateUpdate(UpdateVacancyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Vacancy title is required.");
        if (dto.Title.Length > MaxTitleLength)
            throw new ValidationException($"Vacancy title must not exceed {MaxTitleLength} characters.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ValidationException("Vacancy description is required.");
        if (string.IsNullOrWhiteSpace(dto.Company))
            throw new ValidationException("Company name is required.");
        if (dto.Company.Length > MaxCompanyLength)
            throw new ValidationException($"Company name must not exceed {MaxCompanyLength} characters.");
        if (string.IsNullOrWhiteSpace(dto.RequiredSkills))
            throw new ValidationException("At least one required skill must be specified.");
        if (dto.Salary < 0)
            throw new ValidationException("Salary must be non-negative.");
    }

    private static void EnsureCanManageVacancies(User owner)
    {
        var roleName = owner.Role?.Name ?? string.Empty;
        if (roleName != KnownRoles.Employer && roleName != KnownRoles.Administrator)
            throw new AuthorizationException(
                $"Only Employers and Administrators may manage vacancies. " +
                $"Current role: '{roleName}'.");
    }
}
