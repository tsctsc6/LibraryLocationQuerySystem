using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryLocationQuerySystem.Models
{
    [PrimaryKey("LocationLevel", "LocationId")]
    public class Location
    {
        public byte LocationLevel { get; set; }

        [Range(1, ushort.MaxValue)]
        public ushort LocationId { get; set; }

        public ushort LocationParent { get; set; }

        [Required]
        [StringLength(30)]
        [MaxLength(30)]
        [Display(Name = "位置名")]
        public string LocationName { get; set; }

        [JsonIgnore]
        public List<Book> Books { get; } = new();
        [JsonIgnore]
        public List<Store> Stores { get; } = new();
    }
}
