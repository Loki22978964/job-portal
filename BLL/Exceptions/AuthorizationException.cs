namespace BLL.Exceptions;

public sealed class AuthorizationException : BllException
{
    
    public AuthorizationException() : base("Access denied.") { }

    public AuthorizationException(string message) : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    public AuthorizationException(string message, Exception innerException)
        : base(message, innerException) { }
}
