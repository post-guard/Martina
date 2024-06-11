using System.ComponentModel.DataAnnotations;
using Martina.DataTransferObjects;

namespace Martina.Controllers;

public class BillResponse
{
    /// <summary>
    /// 当前账单的ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 结账的用户名
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 入住的开始时间
    /// </summary>
    [Required]
    public long BeginTime { get; set; }

    /// <summary>
    /// 入住的结束时间
    /// </summary>
    [Required]
    public long EndTime { get; set; }

    /// <summary>
    /// 入住时间段中的入住记录列表
    /// </summary>
    [Required]
    public List<CheckinResponse> CheckinResponses { get; set; } = [];

    /// <summary>
    /// 入住时间段中的空调使用记录列表
    /// </summary>
    [Required]
    public List<AirConditionerRecordResponse> AirConditionerRecordResponses { get; set; } = [];

    /// <summary>
    /// 入住阶段的房费
    /// </summary>
    [Required]
    public decimal RoomFee { get; set; }

    /// <summary>
    /// 入住阶段的空调使用费
    /// </summary>
    [Required]
    public decimal AirConditionerFee { get; set; }
}
