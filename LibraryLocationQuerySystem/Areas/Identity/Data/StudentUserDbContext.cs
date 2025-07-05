using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Areas.Identity.Models;

namespace LibraryLocationQuerySystem.Areas.Identity.Data
{
    public class StudentUserDbContext : IdentityDbContext<StudentUser>
    {
        public StudentUserDbContext(DbContextOptions<StudentUserDbContext> options)
            : base(options)
        {
        }
    }
}
