using System.Collections.Concurrent;
using Martina.DataTransferObjects;
using Martina.Models;
using MongoDB.Bson;

namespace Martina.Abstractions;

/// <summary>
/// 调度器接口
/// </summary>
public interface ISchedular
{
    /// <summary>
    /// 当前各房间空调的状态
    /// </summary>
    public ConcurrentDictionary<ObjectId, AirConditionerState> States { get; }

    /// <summary>
    /// 发送空调服务请求
    /// </summary>
    /// <param name="roomId">空调服务请求的房间</param>
    /// <param name="request">服务请求</param>
    /// <returns></returns>
    public Task SendRequest(ObjectId roomId, AirConditionerRequest request);

    /// <summary>
    /// 重置控制器的状态
    /// </summary>
    /// <returns></returns>
    public Task Reset();
}
