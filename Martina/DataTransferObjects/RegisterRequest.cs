using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

public class RegisterRequest
{
    [Required]
    public required string UserId { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
