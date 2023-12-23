using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class DeleteModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public DeleteModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Book Book { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string? BookSortCallNumber, string? BookFormCallNumber)
        {
            if (BookSortCallNumber == null || BookFormCallNumber == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FirstOrDefaultAsync(m => m.BookSortCallNumber == BookSortCallNumber && m.BookFormCallNumber == BookFormCallNumber);

            if (book == null)
            {
                return NotFound();
            }
            else 
            {
                Book = book;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? BookSortCallNumber, string? BookFormCallNumber)
        {
            if (BookSortCallNumber == null || BookFormCallNumber == null || _context.Book == null)
            {
                return NotFound();
            }
            var book = await _context.Book.FirstOrDefaultAsync(m => m.BookSortCallNumber == BookSortCallNumber && m.BookFormCallNumber == BookFormCallNumber);

            if (book != null)
            {
                Book = book;
                _context.Book.Remove(Book);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
