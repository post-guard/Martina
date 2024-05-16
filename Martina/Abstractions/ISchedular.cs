using System.Collections.Concurrent;
using Martina.DataTransferObjects;
using Martina.Models;
using MongoDB.Bson;

namespace Martina.Abstractions;

public interface ISchedular
{
    public ConcurrentDictionary<ObjectId, AirConditionerState> States { get; }

    public Task SendRequest(ObjectId roomId, AirConditionerRequest request);

    public Task Reset();
}
