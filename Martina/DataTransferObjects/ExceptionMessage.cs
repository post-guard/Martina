using System.ComponentModel.DataAnnotations;
using Martina.Exceptions;

namespace Martina.DataTransferObjects;

public class ExceptionMessage
{
    [Required]
    public string Message { get; set; } = string.Empty;

    public ExceptionMessage()
    {

    }

    public ExceptionMessage(Exception e)
    {
        Message = e.Message;
    }
}
