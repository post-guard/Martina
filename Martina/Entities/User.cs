using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Entities;

/// <summary>
/// 入住用户实体类
/// </summary>
public class User
{
    public ObjectId Id { get; set; }

    [MaxLength(20)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Password { get; set; } = string.Empty;

    public UserPermission Permission { get; set; } = new();
}
