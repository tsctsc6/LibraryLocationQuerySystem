using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using System.Text;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class DetailsModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public DetailsModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        public Book Book { get; set; } = default!;
        public string[] LocationPaths { get; set; } = null;

        public async Task<IActionResult> OnGetAsync(string? BookSortCallNumber, string? BookFormCallNumber)
        {
            if (BookSortCallNumber == null || BookFormCallNumber == null || _context.Book == null || _context.Store == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FirstOrDefaultAsync(m => m.BookSortCallNumber == BookSortCallNumber && m.BookFormCallNumber == BookFormCallNumber);
            if (book == null)
            {
                return NotFound();
            }
            Book = book;

            var stores = await _context.Store.Where(s => s.BookSortCallNumber == Book.BookSortCallNumber &&
                s.BookFormCallNumber == Book.BookFormCallNumber).ToArrayAsync();
            LocationPaths = await Task.WhenAll(stores.Select(async s => await SetLocationPath(s.LocationLevel, s.LocationId)));

            return Page();
        }

        private async Task<string> SetLocationPath(byte LocationLevel, int LocationId)
        {
            if (_context.Location == null) return string.Empty;
            List<string> strings = new();
            while (LocationLevel >= 0 && LocationLevel < 5)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == LocationLevel &&
                    l.LocationId == LocationId).FirstOrDefaultAsync();
                if (loc == null) throw new ArgumentNullException("Location元组not find");
                LocationId = loc.LocationParent;
                LocationLevel--;
                strings.Insert(0, loc.LocationName);
            }
            StringBuilder sb = new("/ ");
            foreach (var item in strings)
            {
                sb.Append(item);
                sb.Append(" / ");
            }
            return sb.ToString();
        }
    }
}
