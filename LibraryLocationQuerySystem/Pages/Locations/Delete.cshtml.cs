using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;

namespace LibraryLocationQuerySystem.Pages.Locations
{
    public class DeleteModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public DeleteModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Location Location { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(byte? LocationLevel, short? LocationId)
        {
			if (LocationLevel == null || LocationId == null || _context.Location == null)
			{
                return NotFound();
            }

			var location = await _context.Location.FirstOrDefaultAsync(
				m => m.LocationLevel == LocationLevel && m.LocationId == LocationId);

			if (location == null)
            {
                return NotFound();
            }
            else 
            {
                Location = location;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(byte? LocationLevel, short? LocationId)
        {
			if (LocationLevel == null || LocationId == null || _context.Location == null)
			{
                return NotFound();
            }
			var location = await _context.Location.FirstOrDefaultAsync(
				m => m.LocationLevel == LocationLevel && m.LocationId == LocationId);

			if (location != null)
            {
                Location = location;
                _context.Location.Remove(Location);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
