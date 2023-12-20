namespace LibraryLocationQuerySystem.Models
{
    public class Store
    {
        public DateTime StoreDate { get; set; }

        public byte StoreNum { get; set; }

        public byte RemainNum { get; set; }

        public Book Book { get; set; } = null!;

        public Location Location { get; set; } = null!;
    }
}
