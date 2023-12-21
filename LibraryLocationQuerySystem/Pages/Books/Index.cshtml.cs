using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using LibraryLocationQuerySystem.Utilities;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

		[BindProperty(SupportsGet = true)]
		public string? SearchString { get; set; }

        public PageManager pm { get; private set; }
        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public int pageNum { get; set; } = 0;

        public IndexModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
            pm = new() { NumPerPage = 20 };
        }

        public IList<Book> Book { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Book == null) return;

			

			pm.Set(pageNum, await _context.Book.CountAsync());
            Book = await _context.Book.Skip(pm.StartIndex).Take(pm.NumPerPage).ToListAsync();
        }
    }
}
