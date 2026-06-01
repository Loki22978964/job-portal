namespace BLL.Exceptions;

public sealed class ValidationException : BllException
{
    
    public ValidationException() : base("Validation failed.") { }

    public ValidationException(string message) : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}
