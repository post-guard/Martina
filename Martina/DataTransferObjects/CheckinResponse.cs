using System.ComponentModel.DataAnnotations;
using Martina.Entities;

namespace Martina.DataTransferObjects;

public class CheckinResponse
{
    /// <summary>
    /// 入住记录ID
    /// </summary>
    [Required]
    public string CheckinId { get; set; } = string.Empty;

    /// <summary>
    /// 入住的房间ID
    /// </summary>
    [Required]
    public string RoomId { get; set; } = string.Empty;

    /// <summary>
    /// 入住的用户ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 入住时间
    /// </summary>
    [Required]
    public long BeginTime { get; set; }

    /// <summary>
    /// 退房时间
    /// </summary>
    [Required]
    public long EndTime { get; set; }

    /// <summary>
    /// 是否已经退房
    /// </summary>
    [Required]
    public bool Checkout { get; set; }

    public CheckinResponse()
    {

    }

    public CheckinResponse(CheckinRecord record)
    {
        CheckinId = record.Id.ToString();
        RoomId = record.RoomId.ToString();
        UserId = record.UserId;
        BeginTime = record.BeginTime.ToUnixTimeSeconds();
        EndTime = record.EndTime.ToUnixTimeSeconds();
        Checkout = record.Checkout;
    }
}
