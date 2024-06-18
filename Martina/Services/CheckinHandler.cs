using System.Security.Claims;
using Martina.Entities;
using Martina.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Martina.Services;

/// <summary>
/// 入住用户的权限验证处理类
/// </summary>
/// <param name="roomService"></param>
/// <param name="dbContext"></param>
public class CheckinHandler(
    RoomService roomService,
    MartinaDbContext dbContext)
    : AuthorizationHandler<CheckinRequirement, Room>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CheckinRequirement requirement,
        Room resource)
    {
        Claim? userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return;
        }

        User? user = await dbContext.Users.AsNoTracking()
            .Where(u => u.UserId == userId.Value)
            .FirstOrDefaultAsync();

        if (user is { Permission.IsAdministrator: true } || user is { Permission.AirConditionorAdministrator: true })
        {
            context.Succeed(requirement);
            return;
        }

        CheckinRecord? record = await roomService.QueryUserCurrentStatus(userId.Value);

        if (record?.RoomId == resource.Id)
        {
            context.Succeed(requirement);
        }
    }
}
