namespace Yf.Pt.Models;

/// <summary>
/// 知识库文章（SOP / 项目文档 / FAQ）
/// </summary>
public class KnowledgeArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Markdown 正文
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// sop / doc / faq
    /// </summary>
    public string Category { get; set; } = "doc";

    public string? Tags { get; set; }

    /// <summary>
    /// 关联项目（可选）
    /// </summary>
    public int? AppProjectId { get; set; }
    public AppProject? AppProject { get; set; }

    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
