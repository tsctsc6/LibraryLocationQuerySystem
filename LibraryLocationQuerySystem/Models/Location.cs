using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibraryLocationQuerySystem.Models
{
    [PrimaryKey("LocationLevel", "LocationId", "LocationParent")]
    public class Location
    {
        public byte LocationLevel { get; set; }

        [Range(1, byte.MaxValue)]
        public byte LocationId { get; set; }

        public byte LocationParent { get; set; }

        [Required]
        [StringLength(30)]
        [MaxLength(30)]
        [Display(Name = "位置名")]
        public string LocationName { get; set; }

        public List<Book> Books { get; } = new();
        public List<Store> Stores { get; } = new();
    }
}
