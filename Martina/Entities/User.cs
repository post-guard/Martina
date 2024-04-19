using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Entities;

public class User
{
    public ObjectId Id { get; set; }

    [MaxLength(20)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Password { get; set; } = string.Empty;
}
