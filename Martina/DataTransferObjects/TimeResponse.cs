using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

public class TimeResponse
{
    /// <summary>
    /// 当前的系统时间
    /// </summary>
    [Required]
    public long Now { get; set; }
}
