using System.ComponentModel.DataAnnotations;

namespace AusDevsBot.Data;

public class User
{
    [Key]
    public ulong UserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public int NumberOfSnippets => SavedSnippets.Count;
    
    public virtual List<SavedSnippet> SavedSnippets { get; set; }
}