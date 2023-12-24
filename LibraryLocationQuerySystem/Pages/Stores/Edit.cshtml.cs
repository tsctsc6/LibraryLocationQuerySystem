using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using LibraryLocationQuerySystem.Utilities;

namespace LibraryLocationQuerySystem.Pages.Stores
{
    public class EditModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public EditModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Store Store { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string? bscn, string? bfcn, byte? ll, int? li)
        {
            if (bscn == null || bfcn == null || ll == null || li == null || _context.Store == null)
            {
                return NotFound();
            }

            var store = await _context.Store.FirstOrDefaultAsync(m => m.BookSortCallNumber == bscn &&
                m.BookFormCallNumber == bfcn && m.LocationLevel == ll && m.LocationId == li);
            if (store == null)
            {
                return NotFound();
            }
            Store = store;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //PrintModelState.printErrorMessage(ModelState);
            /*
            if (!ModelState.IsValid)
            {
                return Page();
            }
            */
            if (Store.StoreNum < 0 || Store.RemainNum > Store.StoreNum) return Page();
            _context.Attach(Store).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(Store.BookSortCallNumber))
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

        private bool StoreExists(string id)
        {
          return (_context.Store?.Any(e => e.BookSortCallNumber == id)).GetValueOrDefault();
        }
    }
}
