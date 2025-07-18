﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Models;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class EditModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.ApplicationDbContext _context;

        public EditModel(LibraryLocationQuerySystem.Data.ApplicationDbContext context)
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

            var book =  await _context.Book.FirstOrDefaultAsync(m => m.BookSortCallNumber == BookSortCallNumber && m.BookFormCallNumber == BookFormCallNumber);
            if (book == null)
            {
                return NotFound();
            }
            Book = book;
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

            _context.Attach(Book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(Book.BookSortCallNumber))
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

        private bool BookExists(string id)
        {
          return (_context.Book?.Any(e => e.BookSortCallNumber == id)).GetValueOrDefault();
        }
    }
}
