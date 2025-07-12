using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LibraryLocationQuerySystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<StudentUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Book> Book { get; set; } = default!;
        public DbSet<Location> Location { get; set; } = default!;
        public DbSet<Store> Store { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
