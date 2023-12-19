using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryLocationQuerySystem.Areas.Identity.Models
{
    public class StudentUser : IdentityUser
    {
        [Column(TypeName = "char(10)")]
        [MaxLength(10)]
        [Required]
        [RegularExpression(@"\d+")]
        [PersonalData]
        public string StudentId { get; set; }

        [Required]
        public bool IsPasswordChanged { get; set; } = false;
    }
}
