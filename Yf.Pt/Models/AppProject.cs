namespace Yf.Pt.Models;

/// <summary>
/// App 项目
/// </summary>
public class AppProject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DomainBinding> DomainBindings { get; set; } = new();
}
