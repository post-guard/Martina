using System.Security.Claims;
using Martina.Enums;
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

        UserPermission? permission = await dbContext.UserPermissions
            .Where(p => p.UserId == userId.Value)
            .FirstOrDefaultAsync();

        if (permission is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "No permission for this user is valid."));
            return;
        }

        if ((requirement.HotelRole & Roles.Administrator) == Roles.Administrator)
        {
            if (permission.IsAdministrator)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        if ((requirement.HotelRole & Roles.BillAdministrator) == Roles.BillAdministrator)
        {
            if (permission.BillAdminstrator)
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
            if (permission.RoomAdministrator)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        if ((requirement.HotelRole & Roles.AirConditionorAdministrator) == Roles.AirConditionorAdministrator)
        {
            if (permission.AirConditionorAdministrator)
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
