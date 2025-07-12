using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LibraryLocationQuerySystem.Pages.Locations
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.ApplicationDbContext _context;

        public IndexModel(LibraryLocationQuerySystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public string LocationPath { get; set; } = "/ ";
        public byte PreviousLevel { get; set; }
        public int PreviousLevelId { get; set; }


		public IList<Location> Location { get; set; } = default!;

        public async Task OnGetAsync(byte? LocationLevel, int? LocationParentId)
        {
            if (_context.Location == null) return;
            await SetPreviousLevel(LocationLevel, LocationParentId);
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
                await SetLocationPath((byte)LocationLevel, (int)LocationParentId);
				Location = await _Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParentId).ToListAsync();
				return;
			}
        }

        private async Task SetLocationPath(byte LocationLevel, int LocationParentId)
        {
			if (_context.Location == null) return;
			List<string> strings = new();
            while(LocationLevel > 0)
            {
				LocationLevel--;
				var loc = await _context.Location.Where(l => l.LocationLevel == LocationLevel &&
                    l.LocationId == LocationParentId).FirstOrDefaultAsync();
                if (loc == null) throw new ArgumentNullException("LocationԪ��not find");
                LocationParentId = loc.LocationParent;
				strings.Insert(0, loc.LocationName);
			}
            StringBuilder sb = new("/ ");
            foreach(var item in strings)
            {
                sb.Append(item);
                sb.Append(" / ");
            }
            LocationPath = sb.ToString();
		}
        private async Task SetPreviousLevel(byte? LocationLevel, int? LocationParentId)
        {
			if (LocationLevel == null || LocationLevel <= 0 || LocationParentId == null)
            {
                PreviousLevel = 0;
                PreviousLevelId = 0;
                return;
			}
			if (LocationLevel > 4)
            {
                PreviousLevel = 4;
                PreviousLevelId = (int)LocationParentId;
                return;
			}
            PreviousLevel = (byte)(LocationLevel - 1);
            PreviousLevelId = (await _context.Location.SingleAsync
                (l => l.LocationLevel == PreviousLevel && l.LocationId == LocationParentId))
                .LocationParent;
		}
	}
}
