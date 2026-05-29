namespace BLL.DTOs;

public sealed record CreateResumeDto(
    
    string Title,
    
    string Description,
    
    string Skills,
    
    decimal ExpectedSalary,
    
    int UserId);
