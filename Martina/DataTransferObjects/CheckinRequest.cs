using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

public class CheckinRequest
{
    /// <summary>
    /// 入住的房间号
    /// </summary>
    [Required]
    public string RoomId { get; set; } = string.Empty;

    /// <summary>
    /// 入住的用户ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 入住的用户姓名
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 入住的时间
    /// </summary>
    [Required]
    public long BeginTime { get; set; }

    /// <summary>
    /// 退房的时间
    /// </summary>
    [Required]
    public long EndTime { get; set; }
}
