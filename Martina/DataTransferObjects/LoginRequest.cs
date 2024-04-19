using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

public class LoginRequest
{
    [Required]
    public required string UserId { get; set; }

    [Required]
    public required string Password { get; set; }
}
