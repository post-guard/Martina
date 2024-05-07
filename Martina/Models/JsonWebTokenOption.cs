namespace Martina.Models;

public class JsonWebTokenOption
{
    public const string OptionName = "JWT";

    /// <summary>
    /// 密码哈希盐
    /// </summary>
    public required string PasswordKey { get; set; }

    /// <summary>
    /// 密码哈希的次数
    /// </summary>
    public required int HashCount { get; set; }

    /// <summary>
    /// JWT令牌的签发者
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// JWT令牌的签发密钥
    /// </summary>
    public required string JsonWebTokenKey { get; set; }
}
