using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using static LibraryLocationQuerySystem.Areas.Identity.Pages.ManageAccounts.MultiRegisterModel;
using Microsoft.EntityFrameworkCore;

namespace LibraryLocationQuerySystem.Pages.Stores
{
    public class MultiCreateModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public MultiCreateModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        public class SelectGroupViewModel
        {
            public int CampusId { get; set; }
            public int LibraryId { get; set; }
            public int FloorId { get; set; }
            public int BookshelfId { get; set; }
            public int LayerId { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public SelectGroupViewModel selectGroupView { get; set; } = new();
        public List<SelectListItem> Campuses { get; set; }
        public List<SelectListItem> Libraries { get; set; }
        public List<SelectListItem> Floors { get; set; }
        public List<SelectListItem> Bookshelves { get; set; }
        public List<SelectListItem> Layers { get; set; }

        [BindProperty]
        public BufferedSingleFileUploadPhysical FileUpload { get; set; }

        public async Task<IActionResult> OnGet()
        {
            await InitSelectGrop();
            return Page();
        }

        [BindProperty]
        public Store Store { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (_context.Store == null || Store == null)
            {
                return Page();
            }
            await InitSelectGrop();
            /*
            _context.Store.Add(Store);
            await _context.SaveChangesAsync();
            */
            return Page();
            //return RedirectToPage("./Index");
        }

        private async Task InitSelectGrop()
        {
            if (_context.Location == null) return;

            Campuses = await _context.Location.Where(l => l.LocationLevel == 0)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Campuses.Insert(0, new("(未选择)", "0"));

            Libraries = await _context.Location.Where(l => l.LocationLevel == 1 && l.LocationParent == selectGroupView.CampusId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Libraries.Insert(0, new("(未选择)", "0"));

            Floors = await _context.Location.Where(l => l.LocationLevel == 2 && l.LocationParent == selectGroupView.LibraryId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Floors.Insert(0, new("(未选择)", "0"));

            Bookshelves = await _context.Location.Where(l => l.LocationLevel == 3 && l.LocationParent == selectGroupView.FloorId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Bookshelves.Insert(0, new("(未选择)", "0"));

            Layers = await _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == selectGroupView.BookshelfId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Layers.Insert(0, new("(未选择)", "0"));
        }
        public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
        {
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
    }
}
