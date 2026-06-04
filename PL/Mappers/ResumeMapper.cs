namespace PL.Mappers;

public static class ResumeMapper
{
    
    public static ResumeViewModel ToViewModel(ResumeDto dto) =>
        new(dto.Id,
            dto.Title,
            dto.Description,
            dto.Skills,
            dto.ExpectedSalary,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.UserId,
            dto.UserFullName);

    public static CreateResumeDto ToCreateDto(CreateResumeRequest request, int userId) =>
        new(request.Title,
            request.Description,
            request.Skills,
            request.ExpectedSalary,
            userId);

    public static UpdateResumeDto ToUpdateDto(UpdateResumeRequest request) =>
        new(request.Title,
            request.Description,
            request.Skills,
            request.ExpectedSalary);

    public static ResumeFilterDto ToFilterDto(ResumeFilterRequest request) =>
        new(request.Keyword,
            request.SortBy,
            request.Ascending,
            request.MinSalary,
            request.MaxSalary);
}
