using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Models;

namespace LibraryLocationQuerySystem.Pages.Locations
{
    public class EditModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.ApplicationDbContext _context;

        public EditModel(LibraryLocationQuerySystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Location Location { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(byte? LocationLevel, int? LocationId)
        {
            if (LocationLevel == null || LocationId == null || _context.Location == null)
            {
                return NotFound();
            }

            var location =  await _context.Location.FirstOrDefaultAsync(
                m => m.LocationLevel == LocationLevel && m.LocationId == LocationId);
            if (location == null)
            {
                return NotFound();
            }
            Location = location;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(Location.LocationLevel, Location.LocationId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool LocationExists(byte LocationLevel, int LocationId)
        {
            return (_context.Location?.Any(m => m.LocationLevel == LocationLevel &&
                m.LocationId == LocationId)).GetValueOrDefault();
        }
    }
}
