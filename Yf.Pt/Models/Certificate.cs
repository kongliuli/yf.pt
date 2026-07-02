namespace Yf.Pt.Models;

/// <summary>
/// SSL 证书
/// </summary>
public class Certificate
{
    public int Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Let's Encrypt";
    public DateTime ExpireAt { get; set; }
    public string? Note { get; set; }

    /// <summary>
    /// 剩余天数
    /// </summary>
    public int DaysRemaining => (int)Math.Ceiling((ExpireAt - DateTime.UtcNow).TotalDays);
}
