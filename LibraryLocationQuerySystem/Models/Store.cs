using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryLocationQuerySystem.Models
{
    public class Store
    {
        public DateTime StoreDate { get; set; }

        [Required]
        public byte StoreNum { get; set; }

        public byte RemainNum { get; set; }

        [JsonIgnore]
        public Book Book { get; set; } = null!;
        [JsonIgnore]
        public Location Location { get; set; } = null!;
    }
}
