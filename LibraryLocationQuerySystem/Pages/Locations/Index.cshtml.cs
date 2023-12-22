using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text;

namespace LibraryLocationQuerySystem.Pages.Locations
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public IndexModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        public string LocationPath { get; set; } = "";

        public IList<Location> Location { get; set; } = default!;

        public async Task OnGetAsync(byte? LocationLevel, short? LocationParentId)
        {
            if (_context.Location == null) return;
            if (LocationLevel > 4)
			{
				Location = new List<Location>();
				return;
			}
			IQueryable<Location> _Location = _context.Location;
            if (LocationLevel == null || LocationLevel == 0)
            {
				Location = await _Location.Where(l => l.LocationLevel == 0).ToListAsync();
                return;
            }
            if (LocationParentId == null)
			{
				Location = new List<Location>();
				return;
			}
			else
            {
                await SetLocationPath((byte)LocationLevel, (short)LocationParentId);
				Location = await _Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParentId).ToListAsync();
				return;
			}
        }

        private async Task SetLocationPath(byte LocationLevel, short LocationParentId)
        {
			if (_context.Location == null) return;
			List<string> strings = new();
            while(LocationLevel > 0)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == LocationLevel &&
                    l.LocationParent == LocationParentId).FirstOrDefaultAsync();
                if (loc == null) throw new ArgumentNullException("LocationÔª×énot find");
                LocationParentId = loc.LocationParent;
				strings.Insert(0, loc.LocationName);
				LocationLevel--;
			}
            StringBuilder sb = new();
            foreach(var item in strings)
            {
                sb.Append(item);
                sb.Append(" / ");
            }
            LocationPath = sb.ToString();
		}

	}
}
