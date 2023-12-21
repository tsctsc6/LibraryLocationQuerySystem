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

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class EditModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public EditModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Book Book { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string? SortCallNumber, string? FormCallNumber)
        {
            if (SortCallNumber == null || FormCallNumber == null || _context.Book == null)
            {
                return NotFound();
            }

            var book =  await _context.Book.FirstOrDefaultAsync(m => m.SortCallNumber == SortCallNumber && m.FormCallNumber == FormCallNumber);
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
            foreach(var item in ModelState)
            {
                Console.WriteLine(item.Key);
                foreach (var item2 in item.Value.Errors)
                {
                    Console.WriteLine(item2.ErrorMessage);
                }
            }
            Console.WriteLine(Book.SortCallNumber);
            Console.WriteLine(Book.FormCallNumber);
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
                if (!BookExists(Book.SortCallNumber))
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
          return (_context.Book?.Any(e => e.SortCallNumber == id)).GetValueOrDefault();
        }
    }
}
