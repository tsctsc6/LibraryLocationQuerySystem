using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryLocationQuerySystem.Models
{
    public class Store
    {
        [StringLength(15)]
        [RegularExpression(@"[0-9a-zA-Z-\.\(\)]")]
        [Display(Name = "中图法分类号")]
        [Column(TypeName = "nchar(15)")]
        public string BookSortCallNumber { get; set; }

        [StringLength(15)]
        [RegularExpression(@"[0-9a-zA-Z-\.\(\)]")]
        [Display(Name = "书次号")]
        [Column(TypeName = "nchar(15)")]
        public string BookFormCallNumber { get; set; }

        public byte LocationLevel { get; set; }

        public int LocationId { get; set; }

        public DateTime StoreDate { get; set; }

        [Required]
        public byte StoreNum { get; set; }

        public byte RemainNum { get; set; }

        [Column(TypeName = "char(10)")]
        [MaxLength(10)]
        [Required]
        [RegularExpression(@"\d+")]
        [JsonIgnore]
        public string ManageBy { get; set; }

        [JsonIgnore]
        public Book Book { get; set; } = null!;
        [JsonIgnore]
        public Location Location { get; set; } = null!;
    }
}
