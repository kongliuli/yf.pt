namespace Yf.Pt.Models;

/// <summary>
/// 本地资源：自建服务器/物理机/内网服务
/// </summary>
public class LocalResource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// server / vm / service
    /// </summary>
    public string Type { get; set; } = "server";

    public string? Ip { get; set; }
    public string? Location { get; set; }
    public string? Owner { get; set; }
    public string? Note { get; set; }
}
