namespace BLL.DTOs;

public sealed record OperationResult<T>
{
    
    public bool IsSuccess { get; init; }

    public T? Data { get; init; }

    public string? ErrorMessage { get; init; }
}

public static class OperationResult
{
    
    public static OperationResult<T> Success<T>(T data) =>
        new() { IsSuccess = true, Data = data };

    public static OperationResult<T> Failure<T>(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);
        return new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
