using System.Security.Claims;
using Martina.Entities;
using Martina.Models;
using Microsoft.AspNetCore.Authorization;

namespace Martina.Services;

public class CheckinHandler(RoomService roomService) : AuthorizationHandler<CheckinRequirement, Room>
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

        CheckinRecord? record = await roomService.QueryUserCurrentStatus(userId.Value);

        if (record is null || record.RoomId != resource.Id)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
