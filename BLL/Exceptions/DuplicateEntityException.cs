namespace BLL.Exceptions;

public sealed class DuplicateEntityException : BllException
{
    
    public DuplicateEntityException() : base("A duplicate entity was detected.") { }

    public DuplicateEntityException(string message) : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    public DuplicateEntityException(string message, Exception innerException)
        : base(message, innerException) { }
}
