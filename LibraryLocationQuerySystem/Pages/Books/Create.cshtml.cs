using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using LibraryLocationQuerySystem.Areas.Identity.Data;
using LibraryLocationQuerySystem.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryLocationQuerySystem.Pages.Books
{
    public class CreateModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;
        private readonly UserManager<StudentUser> _userManager;

        public CreateModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context, UserManager<StudentUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
