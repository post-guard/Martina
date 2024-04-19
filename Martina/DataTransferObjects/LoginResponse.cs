using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

public class LoginResponse
{
    [Required]
    public required string Token { get; set; }
}
