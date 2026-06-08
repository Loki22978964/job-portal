namespace BLL.Services;

public sealed class ApplicationService : IApplicationService
{
    private readonly IUnitOfWork _unitOfWork;

    public ApplicationService(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<ApplicationDto>> ApplyAsync(
        ApplyDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var resume = await _unitOfWork.Resumes
            .GetByIdAsync(dto.ResumeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), dto.ResumeId);

        if (resume.UserId != dto.ApplicantUserId)
            throw new AuthorizationException(
                "You can only apply with a resume that belongs to your own account.");

        _ = await _unitOfWork.Vacancies
            .GetByIdAsync(dto.VacancyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), dto.VacancyId);

        bool duplicate = await _unitOfWork.Applications
            .ExistsAsync(dto.ResumeId, dto.VacancyId, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new DuplicateEntityException(
                $"An application already exists for resume {dto.ResumeId} and vacancy {dto.VacancyId}.");

        var application = new Application
        {
            ResumeId = dto.ResumeId,
            VacancyId = dto.VacancyId,
            Type = ApplicationType.Apply,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow,
        };

        await _unitOfWork.Applications.AddAsync(application, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await LoadApplicationAsync(application.Id, cancellationToken).ConfigureAwait(false);
        return OperationResult.Success(ApplicationMapper.ToDto(created));
    }

    public async Task<OperationResult<ApplicationDto>> ProposeAsync(
        ProposeDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var vacancy = await _unitOfWork.Vacancies
            .GetByIdAsync(dto.VacancyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), dto.VacancyId);

        if (vacancy.UserId != dto.ProposerUserId)
            throw new AuthorizationException(
                "You can only propose with a vacancy that belongs to your own account.");

        _ = await _unitOfWork.Resumes
            .GetByIdAsync(dto.ResumeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), dto.ResumeId);

        bool duplicate = await _unitOfWork.Applications
            .ExistsAsync(dto.ResumeId, dto.VacancyId, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new DuplicateEntityException(
                $"An application already exists for resume {dto.ResumeId} and vacancy {dto.VacancyId}.");

        var application = new Application
        {
            ResumeId = dto.ResumeId,
            VacancyId = dto.VacancyId,
            Type = ApplicationType.Propose,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow,
        };

        await _unitOfWork.Applications.AddAsync(application, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await LoadApplicationAsync(application.Id, cancellationToken).ConfigureAwait(false);
        return OperationResult.Success(ApplicationMapper.ToDto(created));
    }

    public async Task<OperationResult<ApplicationDto>> AcceptAsync(
        int id, int userId, CancellationToken cancellationToken = default)
    {
        var application = await LoadApplicationAsync(id, cancellationToken).ConfigureAwait(false);

        if (application.Status != ApplicationStatus.Pending)
            throw new ValidationException("Only pending applications can be accepted.");

        var user = await _unitOfWork.Users
            .GetByIdWithRoleAsync(userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), userId);

        if (user.RoleId != KnownRoles.AdministratorId)
        {
            if (application.Type == ApplicationType.Apply)
            {
                if (application.Vacancy?.UserId != userId)
                    throw new AuthorizationException("Only the vacancy owner can accept this application.");
            }
            else
            {
                if (application.Resume?.UserId != userId)
                    throw new AuthorizationException("Only the resume owner can accept this proposal.");
            }
        }

        application.Status = ApplicationStatus.Accepted;
        _unitOfWork.Applications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult.Success(ApplicationMapper.ToDto(application));
    }

    public async Task<OperationResult<ApplicationDto>> RejectAsync(
        int id, int userId, CancellationToken cancellationToken = default)
    {
        var application = await LoadApplicationAsync(id, cancellationToken).ConfigureAwait(false);

        if (application.Status != ApplicationStatus.Pending)
            throw new ValidationException("Only pending applications can be rejected.");

        var user = await _unitOfWork.Users
            .GetByIdWithRoleAsync(userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(User), userId);

        if (user.RoleId != KnownRoles.AdministratorId)
        {
            if (application.Type == ApplicationType.Apply)
            {
                if (application.Vacancy?.UserId != userId)
                    throw new AuthorizationException("Only the vacancy owner can reject this application.");
            }
            else
            {
                if (application.Resume?.UserId != userId)
                    throw new AuthorizationException("Only the resume owner can reject this proposal.");
            }
        }

        application.Status = ApplicationStatus.Rejected;
        _unitOfWork.Applications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult.Success(ApplicationMapper.ToDto(application));
    }

    public async Task<IReadOnlyList<VacancyDto>> GetLinkedVacanciesAsync(
        int resumeId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Resumes
            .GetByIdAsync(resumeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Resume), resumeId);

        var applications = await _unitOfWork.Applications
            .GetByResumeIdAsync(resumeId, cancellationToken)
            .ConfigureAwait(false);

        return [.. applications
            .Where(a => a.Vacancy is not null)
            .Select(a => VacancyMapper.ToDto(a.Vacancy!))];
    }

    public async Task<IReadOnlyList<ResumeDto>> GetLinkedResumesAsync(
        int vacancyId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Vacancies
            .GetByIdAsync(vacancyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Vacancy), vacancyId);

        var applications = await _unitOfWork.Applications
            .GetByVacancyIdAsync(vacancyId, cancellationToken)
            .ConfigureAwait(false);

        return [.. applications
            .Where(a => a.Resume is not null)
            .Select(a => ResumeMapper.ToDto(a.Resume!))];
    }

    private async Task<Application> LoadApplicationAsync(int id, CancellationToken cancellationToken)
    {
        
        var app = await _unitOfWork.Applications
            .GetByIdWithDetailsAsync(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EntityNotFoundException(nameof(Application), id);

        return app;
    }
}
