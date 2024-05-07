using System.ComponentModel.DataAnnotations;
using Martina.Exceptions;

namespace Martina.DataTransferObjects;

/// <summary>
/// 错误信息传输类
/// </summary>
public class ExceptionMessage
{
    /// <summary>
    /// 错误信息
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    public ExceptionMessage()
    {

    }

    public ExceptionMessage(Exception e)
    {
        Message = e.Message;
    }

    public ExceptionMessage(string message)
    {
        Message = message;
    }
}
