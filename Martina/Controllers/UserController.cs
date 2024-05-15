using Martina.DataTransferObjects;
using Martina.Exceptions;
using Martina.Entities;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Martina.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(UserService userService, RoomService roomService, MartinaDbContext dbContext)
    : ControllerBase
{
    /// <summary>
    /// 注册用户
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 获得用户信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    [Authorize]
    [ProducesResponseType<UserResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(404)]
    public async Task<IActionResult> GetUserInformation([FromRoute] string userId)
    {
        User? user = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound(new ExceptionMessage("查询的用户不存在"));
        }

        CheckinRecord? record = await roomService.QueryUserCurrentStatus(userId);

        if (record is null)
        {
            return Ok(new UserResponse(user));
        }

        return Ok(new UserResponse(user, record));
    }

    /// <summary>
    /// 获得所有的用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Authorize(policy: "Administrator")]
    [ProducesResponseType<IEnumerable<UserResponse>>(200)]
    public IActionResult GetAllUserInformation()
    {
        List<User> users = dbContext.Users
            .AsNoTracking()
            .ToList();

        return Ok(users.Select(u => new UserResponse(u)));
    }

    /// <summary>
    /// 修改用户信息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userResponse"></param>
    /// <returns></returns>
    [HttpPut("{userId}")]
    [Authorize(policy: "Administrator")]
    [ProducesResponseType<UserResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> UpdateUserInformation([FromRoute] string userId,
        [FromBody] UserResponse userResponse)
    {
        if (userId != userResponse.UserId)
        {
            return BadRequest(new ExceptionMessage("Target userId didn't match UserResponse."));
        }

        if (userResponse.Permission.Administrator)
        {
            return BadRequest(new ExceptionMessage("Cann't change a user to administrator."));
        }

        try
        {
            await userService.UpdateUserInformation(userResponse);
        }
        catch (UserException e)
        {
            return BadRequest(new ExceptionMessage(e));
        }

        return Ok(userResponse);
    }
}
