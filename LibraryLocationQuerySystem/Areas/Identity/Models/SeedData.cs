using LibraryLocationQuerySystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryLocationQuerySystem.Areas.Identity.Models
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new StudentUserDbContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<StudentUserDbContext>>()))
            {
                if (context == null)
                    throw new ArgumentNullException("Null StudentUserDbContext");

                if (context.Roles == null)
                    throw new ArgumentNullException("Null StudentUserDbContext.Roles");
                if (context.Roles.Any()) return;   // DB.Roles has been seeded
                context.Roles.AddRange(
                    new IdentityRole("admin") { NormalizedName = "ADMIN" },
                    new IdentityRole("reader") { NormalizedName = "READER" }
                );

                context.SaveChanges();
            }
        }
    }
}
