using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace LibraryLocationQuerySystem.Models
{
    [PrimaryKey("LocationLevel", "LocationId")]
    public class Location
    {
        public byte LocationLevel { get; set; }

        public int LocationId { get; set; }

        public int LocationParent { get; set; }

        [Required]
        [StringLength(30)]
        [MaxLength(30)]
        [Display(Name = "位置名")]
        public string LocationName { get; set; }

        [Column(TypeName = "char(10)")]
        [MaxLength(10)]
        [Required]
        [RegularExpression(@"\d+")]
        [JsonIgnore]
        public string ManageBy { get; set; }

        [JsonIgnore]
        public List<Book> Books { get; } = new();
        [JsonIgnore]
        public List<Store> Stores { get; } = new();

        public void GenLocationid()
        {
            var r = RandomNumberGenerator.GetBytes(1);
            byte[] intBytes = new byte[4] {
                (byte)((LocationParent & 0xff000000) >> 24),
                (byte)((LocationParent & 0x00ff0000) >> 16),
                (byte)((LocationParent & 0x0000ff00) >> 08),
                (byte)((LocationParent & 0x000000ff) >> 00),
            };
            byte[] Bytes = Encoding.Unicode.GetBytes(LocationName).Append(LocationLevel).Concat(intBytes).Concat(r).ToArray();
            byte[] hashValue;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                hashValue = mySHA256.ComputeHash(Bytes);
            }
            byte r2 = (byte)(r[0] & 0b0111);
            byte[] hashValue2 = hashValue.Skip(r2 * 4).Take(4).ToArray();
            if ((r[0] & 0b1000) == 0) LocationId = hashValue2[0] << 24 | hashValue2[1] << 16 | hashValue2[2] << 8 | hashValue2[3] << 0;
            else LocationId = hashValue2[3] << 24 | hashValue2[2] << 16 | hashValue2[1] << 8 | hashValue2[0] << 0;
        }
    }
}
