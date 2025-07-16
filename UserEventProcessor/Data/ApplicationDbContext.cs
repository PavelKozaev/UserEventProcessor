using Microsoft.EntityFrameworkCore;
using UserEventProcessor.Models;

namespace UserEventProcessor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserEventStat> UserEventStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var statEntity = modelBuilder.Entity<UserEventStat>();

            statEntity.HasKey(e => new { e.UserId, e.EventType });

            statEntity.Property(e => e.UserId).HasColumnName("user_id");
            statEntity.Property(e => e.EventType).HasColumnName("event_type");
            statEntity.Property(e => e.Count).HasColumnName("count");

            statEntity.ToTable("user_event_stats");
        }
    }
}
