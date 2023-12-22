using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

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

        public async Task OnGetAsync(byte? LocationLevel, byte? LocationId, byte? LocationParent)
        {
            if (_context.Location == null) return;
            IQueryable<Location> _Location = _context.Location;
            if (LocationLevel == null || LocationLevel == 0)
            {
                Location = await _Location.Where(l => l.LocationLevel == 0).ToListAsync();
                return;
            }
        }
    }
}
