using CodeBase.EFCore.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace CodeBase.EFCore.Data.DB;

public class DatabaseContext : DbContext, IBaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
    
    public void MarkAsModified(object o, string propertyName)
    {
        this.Entry(o).Property(propertyName).IsModified = true;
    }
    
    public virtual DbSet<Player> Players { get; set; }
    public virtual DbSet<Quest> Quests { get; set; }
    public virtual DbSet<Milestone> Milestones { get; set; }
    public virtual DbSet<PlayerQuestState> PlayerQuestState { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Milestone>()
            .HasOne(m => m.Quest)
            .WithMany(q => q.Milestones)
            .HasForeignKey(m => m.QuestId);

        modelBuilder.Entity<PlayerQuestState>()
            .HasKey(pqs => new { pqs.PlayerId, pqs.QuestId });

        modelBuilder.Entity<PlayerQuestState>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(pqs => pqs.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerQuestState>()
            .HasOne<Quest>()
            .WithMany()
            .HasForeignKey(pqs => pqs.QuestId);
    }
}