using System.ComponentModel.DataAnnotations;
using Martina.Entities;

namespace Martina.DataTransferObjects;

public class UserResponse
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    public UserResponse()
    {

    }

    public UserResponse(User user)
    {
        UserId = user.UserId;
        Username = user.Username;
    }
}
