using System.Security.Claims;
using Martina.Enums;
using Martina.Entities;
using Martina.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Martina.Services;

public class HotelRoleHandler(MartinaDbContext dbContext) : AuthorizationHandler<HotelRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        HotelRoleRequirement requirement)
    {
        Claim? userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return;
        }

        User? user = await dbContext.Users
            .Include(u => u.Permission)
            .Where(u => u.UserId == userId.Value)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return;
        }

        // 如果要求的权限是超级管理员
        // 则判断是否是超级管理员
        if ((requirement.HotelRole & Roles.Administrator) == Roles.Administrator)
        {
            if (user.Permission.IsAdministrator)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        // 剩下的权限
        // 如果用户是超级管理员则直接有权限
        if (user.Permission.IsAdministrator)
        {
            context.Succeed(requirement);
            return;
        }

        if ((requirement.HotelRole & Roles.BillAdministrator) == Roles.BillAdministrator)
        {
            if (user.Permission.BillAdminstrator)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        if ((requirement.HotelRole & Roles.RoomAdministrator) == Roles.RoomAdministrator)
        {
            if (user.Permission.RoomAdministrator)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        if ((requirement.HotelRole & Roles.AirConditionerAdministrator) == Roles.AirConditionerAdministrator)
        {
            if (user.Permission.AirConditionorAdministrator)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
