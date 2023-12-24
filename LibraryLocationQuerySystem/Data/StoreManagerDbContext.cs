using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Models;

namespace LibraryLocationQuerySystem.Data
{
    public class StoreManagerDbContext : DbContext
    {
        public StoreManagerDbContext(DbContextOptions<StoreManagerDbContext> options)
            : base(options)
        {
        }
        public DbSet<Book> Book { get; set; } = default!;
        public DbSet<Location> Location { get; set; } = default!;
        public DbSet<Store> Store { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasMany(e => e.Locations)
                .WithMany(e => e.Books).UsingEntity<Store>(
                    l => l.HasOne(e => e.Location).WithMany(e => e.Stores)
                        .HasForeignKey("LocationLevel", "LocationId"),
                    r => r.HasOne(e => e.Book).WithMany(e => e.Stores)
                        .HasForeignKey("BookSortCallNumber", "BookFormCallNumber"),
                    j => j.Property(e => e.StoreDate).HasDefaultValueSql("CURRENT_TIMESTAMP"));
            modelBuilder.Entity<Book>().Property(e => e.Type).HasConversion<byte>();
        }
    }
}
