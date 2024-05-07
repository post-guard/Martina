using Martina.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Martina.Models;

/// <summary>
/// 对于酒店管理权限的要求
/// </summary>
/// <param name="hotelRole">指定的操作权限</param>
public class HotelRoleRequirement(Roles hotelRole) : IAuthorizationRequirement
{
    public Roles HotelRole { get; } = hotelRole;
}
