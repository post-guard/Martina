namespace Martina.Exceptions;

public class BillException : Exception
{
    public BillException()
    {
    }

    public BillException(string message) : base(message)
    {
    }

    public BillException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
