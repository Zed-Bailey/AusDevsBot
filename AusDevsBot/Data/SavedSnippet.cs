using System.ComponentModel.DataAnnotations;

namespace AusDevsBot.Data;

public class SavedSnippet
{
    [Key]
    public Guid SnippetId { get; set; }
    
    public string Content { get; set; }
    public string Language { get; set; }
    
    public string? QuickSaveId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; }
    
    
    public ulong UserId { get; set; }
    public virtual User User { get; set; }
}