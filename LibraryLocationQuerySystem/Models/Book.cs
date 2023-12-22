using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryLocationQuerySystem.Models
{
    [PrimaryKey("SortCallNumber", "FormCallNumber")]
    public class Book
    {
        [StringLength(15)]
        [RegularExpression(@"[0-9a-zA-Z-\.\(\)\s]+")]
        [Display(Name = "中图法分类号")]
        [Column(TypeName = "nchar(15)")]
        public string SortCallNumber { get; set; }

        [StringLength(15)]
        [RegularExpression(@"[0-9a-zA-Z-\.\(\)\s]+")]
        [Display(Name = "书次号")]
        [Column(TypeName = "nchar(15)")]
        public string FormCallNumber { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "书名")]
        [MaxLength(200)]
        public string BookName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "出版社")]
        [MaxLength(100)]
        public string PublishingHouse { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "作者")]
        [MaxLength(100)]
        public string Author { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "出版日期")]
        public DateTime PublicDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "停止发行日期")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0, 4)]
        [Display(Name = "类型")]
        public byte Type { get; set; }

        [JsonIgnore]
        public List<Location> Locations { get; } = new();
        [JsonIgnore]
        public List<Store> Stores { get; } = new();
    }
}
