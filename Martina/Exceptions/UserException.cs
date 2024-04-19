namespace Martina.Exceptions;

public class UserException : Exception
{
    public UserException() : base()
    {

    }

    public UserException(string message) : base(message)
    {

    }

    public UserException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
