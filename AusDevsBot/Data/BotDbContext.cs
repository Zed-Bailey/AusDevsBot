using Microsoft.EntityFrameworkCore;

namespace AusDevsBot.Data;

public class BotDbContext : DbContext
{

    public DbSet<User> Users { get; set; }
    public DbSet<SavedSnippet> SavedSnippets { get; set; }
    
    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }
    
}