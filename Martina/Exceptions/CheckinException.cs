namespace Martina.Exceptions;

public class CheckinException : Exception
{
    public CheckinException()
    {
    }

    public CheckinException(string message) : base(message)
    {
    }

    public CheckinException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
