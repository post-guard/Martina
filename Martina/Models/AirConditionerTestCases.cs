using Martina.DataTransferObjects;
using Martina.Enums;

namespace Martina.Models;

public static class AirConditionerTestCases
{
    public static readonly List<(string, decimal)> HotRooms =
    [
        ("hot-test-1", 10),
        ("hot-test-2", 15),
        ("hot-test-3", 18),
        ("hot-test-4", 12),
        ("hot-test-5", 14)
    ];

    public static readonly List<(string, int)> HotCheckinRecords =
    [
        ("hot-test-1", 2),
        ("hot-test-2", 1),
        ("hot-test-3", 1),
        ("hot-test-4", 1),
        ("hot-test-5", 2)
    ];

    public static readonly List<Dictionary<string, AirConditionerRequest>> HotCases =
    [
        // 0
        new Dictionary<string, AirConditionerRequest>(),
        // 1
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle } }
        },
        // 2
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.Middle } },
            { "hot-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.Middle } }
        },
        // 3
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle } }
        },
        // 4
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle } },
            { "hot-test-4", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle } },
            { "hot-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle } }
        },
        // 5
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 27, Speed = FanSpeed.Middle } },
            { "hot-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.High } }
        },
        // 6
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.High } }
        },
        // 7
        new Dictionary<string, AirConditionerRequest>(),
        // 8
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.High } }
        },
        // 9
        new Dictionary<string, AirConditionerRequest>(),
        // 10
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 28, Speed = FanSpeed.High } },
            { "hot-test-4", new AirConditionerRequest { Open = true, TargetTemperature = 28, Speed = FanSpeed.High } }
        },
        // 11
        new Dictionary<string, AirConditionerRequest>(),
        // 12
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.Middle } }
        },
        // 13
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.High } }
        },
        // 14
        new Dictionary<string, AirConditionerRequest>(),
        // 15
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = false, TargetTemperature = 28, Speed = FanSpeed.High } },
            { "hot-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 27, Speed = FanSpeed.Low } }
        },
        // 16
        new Dictionary<string, AirConditionerRequest>(),
        // 17
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-5", new AirConditionerRequest { Open = false, TargetTemperature = 24, Speed = FanSpeed.Middle } }
        },
        // 18
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 27, Speed = FanSpeed.High } }
        },
        // 19
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 28, Speed = FanSpeed.High } },
            { "hot-test-4", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle } }
        },
        // 20
        new Dictionary<string, AirConditionerRequest>(),
        // 21
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 27, Speed = FanSpeed.Middle } },
            { "hot-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.Middle } }
        },
        // 22
        new Dictionary<string, AirConditionerRequest>(),
        // 23
        new Dictionary<string, AirConditionerRequest>(),
        // 24
        new Dictionary<string, AirConditionerRequest>(),
        // 25
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-1", new AirConditionerRequest { Open = false, TargetTemperature = 22, Speed = FanSpeed.Middle } },
            { "hot-test-3", new AirConditionerRequest { Open = false, TargetTemperature = 27, Speed = FanSpeed.High } },
            { "hot-test-5", new AirConditionerRequest { Open = false, TargetTemperature = 22, Speed = FanSpeed.Middle } }
        },
        // 26
        new Dictionary<string, AirConditionerRequest>
        {
            { "hot-test-2", new AirConditionerRequest { Open = false, TargetTemperature = 27, Speed = FanSpeed.Middle } },
            { "hot-test-4", new AirConditionerRequest { Open = false, TargetTemperature = 25, Speed = FanSpeed.Middle } }
        }
    ];

    public static readonly List<(string, decimal)> CoolRooms =
    [
        ("cool-test-1", 32),
        ("cool-test-2", 28),
        ("cool-test-3", 30),
        ("cool-test-4", 29),
        ("cool-test-5", 35)
    ];

    public static readonly List<(string, int)> CoolCheckinRecords =
    [
        ("cool-test-1", 2),
        ("cool-test-2", 1),
        ("cool-test-3", 1),
        ("cool-test-4", 1),
        ("cool-test-5", 2)
    ];

    public static List<Dictionary<string, AirConditionerRequest>> CoolCases =
    [
        // 0
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle }
            }
        },
        // 1
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 18, Speed = FanSpeed.Middle }
            },
            {
                "cool-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle }
            },
            { "cool-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle } }
        },
        // 2
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle }
            }
        },
        // 3
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 19, Speed = FanSpeed.Middle }
            },
            {
                "cool-test-4", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.Middle }
            }
        },
        // 4
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle }
            }
        },
        // 5
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 18, Speed = FanSpeed.High }
            }
        },
        // 6
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = false, TargetTemperature = 19, Speed = FanSpeed.Middle }
            }
        },
        // 7
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 19, Speed = FanSpeed.Middle }
            },
            {
                "cool-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.High }
            }
        },
        // 8
        new Dictionary<string, AirConditionerRequest>(),
        // 9
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.High }
            },
            { "cool-test-4", new AirConditionerRequest { Open = true, TargetTemperature = 18, Speed = FanSpeed.High } }
        },
        // 10
        new Dictionary<string, AirConditionerRequest>(),
        // 11
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle }
            }
        },
        // 12
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Low }
            }
        },
        // 13
        new Dictionary<string, AirConditionerRequest>(),
        // 14
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = false, TargetTemperature = 22, Speed = FanSpeed.High }
            },
            { "cool-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.Low } }
        },
        // 15
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 20, Speed = FanSpeed.High }
            }
        },
        // 16
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = false, TargetTemperature = 22, Speed = FanSpeed.Middle }
            }
        },
        // 17
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-3", new AirConditionerRequest { Open = true, TargetTemperature = 24, Speed = FanSpeed.High }
            }
        },
        // 18
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.High }
            },
            {
                "cool-test-4", new AirConditionerRequest { Open = true, TargetTemperature = 20, Speed = FanSpeed.Middle }
            }
        },
        // 19
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = true, TargetTemperature = 22, Speed = FanSpeed.Middle }
            }
        },
        // 20
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-5", new AirConditionerRequest { Open = true, TargetTemperature = 25, Speed = FanSpeed.High }
            }
        },
        // 21
        new Dictionary<string, AirConditionerRequest>(),
        // 22
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-3", new AirConditionerRequest { Open = false, TargetTemperature = 24, Speed = FanSpeed.High }
            }
        },
        // 23
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-5", new AirConditionerRequest { Open = false, TargetTemperature = 24, Speed = FanSpeed.High }
            },
        },
        // 24
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-1", new AirConditionerRequest { Open = false, TargetTemperature = 24, Speed = FanSpeed.High }
            },
        },
        // 25
        new Dictionary<string, AirConditionerRequest>
        {
            {
                "cool-test-2", new AirConditionerRequest { Open = false, TargetTemperature = 24, Speed = FanSpeed.High }
            },
            { "cool-test-4", new AirConditionerRequest { Open = false, TargetTemperature = 24, Speed = FanSpeed.High } },
        }
    ];
}
