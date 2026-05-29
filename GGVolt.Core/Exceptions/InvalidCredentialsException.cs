namespace GGVolt.Core.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() 
        : base("Неверный email или пароль.") { }
}