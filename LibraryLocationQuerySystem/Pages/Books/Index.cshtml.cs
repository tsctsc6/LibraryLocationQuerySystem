using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Models;
using System.ComponentModel.DataAnnotations;
using LibraryLocationQuerySystem.Utilities;
using System.ComponentModel;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

		[BindProperty(SupportsGet = true)]
        [DisplayName("搜索中图法分类号")]
		public bool SearchBookBookSortCallNumber { get; set; }

		[BindProperty(SupportsGet = true)]
		[DisplayName("搜索书次号")]
		public bool SearchBookBookFormCallNumber { get; set; }

		[BindProperty(SupportsGet = true)]
		[DisplayName("搜索书名")]
		public bool SearchBookName { get; set; } = true;

		[BindProperty(SupportsGet = true)]
		[DisplayName("搜索出版社")]
		public bool SearchPublishingHouse { get; set; }

		[BindProperty(SupportsGet = true)]
		[DisplayName("搜索作者")]
		public bool SearchBookAuthor { get; set; }

		[BindProperty(SupportsGet = true)]
		public string? SearchString { get; set; }

        public PageManager pm { get; set; } = new() { NumPerPage = 20 };
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

            IQueryable<Book> _Book = _context.Book;

			if (!string.IsNullOrEmpty(SearchString))
            {
                _Book = _Book.Where(b => 
                    (SearchBookBookSortCallNumber && b.BookSortCallNumber.Contains(SearchString)) ||
                    (SearchBookBookFormCallNumber && b.BookFormCallNumber.Contains(SearchString)) ||
                    (SearchBookName && b.BookName.Contains(SearchString)) ||
					(SearchPublishingHouse && b.PublishingHouse.Contains(SearchString)) ||
					(SearchBookAuthor && b.Author.Contains(SearchString))
				);
			}

			pm.Set(pageNum, await _context.Book.CountAsync());
            Book = await _Book.OrderBy(b => b.BookSortCallNumber + b.BookFormCallNumber).Skip(pm.StartIndex).Take(pm.NumPerPage).ToListAsync();
        }
    }
}
