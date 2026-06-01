namespace BLL.Exceptions;

public sealed class EntityNotFoundException : BllException
{
    
    public EntityNotFoundException() : base("The requested entity was not found.") { }

    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with id {id} was not found.")
    {
        ArgumentNullException.ThrowIfNull(entityName);
    }

    public EntityNotFoundException(string message) : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    public EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
