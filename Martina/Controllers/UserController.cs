using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Exceptions;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Martina.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService, MartinaDbContext dbContext) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            await userService.Register(request);
            return Ok();
        }
        catch (UserException e)
        {
            return BadRequest(new ExceptionMessage(e));
        }
    }

    [HttpPost("login")]
    [ProducesResponseType<ExceptionMessage>(400)]
    [ProducesResponseType<LoginResponse>(200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            string token = await userService.Login(request);

            return Ok(new LoginResponse { Token = token });
        }
        catch (UserException e)
        {
            return BadRequest(new ExceptionMessage(e));
        }
    }

    [HttpGet("{userId}")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<ActionResult<UserResponse>> GetUserInformation([FromRoute] string userId)
    {
        User? user = await dbContext.Users
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserResponse(user));
    }
}
