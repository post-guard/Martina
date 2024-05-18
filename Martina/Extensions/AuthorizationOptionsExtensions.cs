using Martina.Enums;
using Martina.Models;
using Microsoft.AspNetCore.Authorization;

namespace Martina.Extensions;

public static class AuthorizationOptionsExtensions
{
    /// <summary>
    /// 添加酒店角色相关的认证策略
    /// </summary>
    public static void AddHotelRoleRequirement(this AuthorizationOptions options)
    {
        options.AddPolicy("Administrator", policy =>
        {
            policy.AddRequirements(new HotelRoleRequirement(Roles.Administrator));
        });

        options.AddPolicy("RoomAdministrator", policy =>
            policy.AddRequirements(new HotelRoleRequirement(Roles.RoomAdministrator)));

        options.AddPolicy("AirConditionerAdministrator", policy =>
            policy.AddRequirements(new HotelRoleRequirement(Roles.AirConditionerAdministrator)));

        options.AddPolicy("BillAdministrator", policy =>
            policy.AddRequirements(new HotelRoleRequirement(Roles.BillAdministrator)));
    }
}
