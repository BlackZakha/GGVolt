namespace GGVolt.Core.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id) 
        : base($"Сущность '{entityName}' с ID {id} не найдена.") { }
    
    public EntityNotFoundException(string message) : base(message) { }
}