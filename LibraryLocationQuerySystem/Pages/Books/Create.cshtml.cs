using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibraryLocationQuerySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class CreateModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public CreateModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Book Book { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Book == null || Book == null)
            {
                return Page();
            }
            Book.ManageBy = User?.Identity?.Name;
            await _context.Book.AddAsync(Book);
            try { await _context.SaveChangesAsync(); }
            catch(DbUpdateException e)
            {
                ModelState.AddModelError(string.Empty, e.InnerException?.Message??string.Empty);
                return Page();
            }
            

            return RedirectToPage("./Index");
        }
    }
}
