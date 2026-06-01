namespace BLL.Exceptions;

public abstract class BllException : Exception
{
    
    protected BllException() { }

    protected BllException(string message) : base(message) { }

    protected BllException(string message, Exception innerException) : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
    }
}
