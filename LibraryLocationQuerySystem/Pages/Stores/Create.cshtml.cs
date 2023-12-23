using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using static LibraryLocationQuerySystem.Pages.Stores.IndexModel;
using Microsoft.EntityFrameworkCore;

namespace LibraryLocationQuerySystem.Pages.Stores
{
    public class CreateModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public CreateModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        public class InputModel
        {
            public SelectGroupViewModel selectGroupView { get; set; } = new();
            public List<SelectListItem> Campuses { get; set; }
            public List<SelectListItem> Libraries { get; set; }
            public List<SelectListItem> Floors { get; set; }
            public List<SelectListItem> Bookshelves { get; set; }
            public List<SelectListItem> Layers { get; set; }
            public List<SelectListItem> IfConflict { get; set; }
        }
        public class SelectGroupViewModel
        {
            public short CampusId { get; set; }
            public short LibraryId { get; set; }
            public short FloorId { get; set; }
            public short BookshelfId { get; set; }
            public short LayerId { get; set; }
            public int IfConflict { get; set; }
        }

        [BindProperty]
        public Store Store { get; set; } = default!;

        [BindProperty]
        public Book Book { get; set; } = default!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await InitSelectGrop();
            ViewData["AlertMessage"] = null;
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            await InitSelectGrop();
            /*
            if (!ModelState.IsValid || _context.Store == null || Store == null)
            {
                return Page();
            }
        
            _context.Store.Add(Store);
            await _context.SaveChangesAsync();
            */
            ViewData["AlertMessage"] = "aaaa";
            //return RedirectToPage("./Index");
            return Page();
        }

        private async Task InitSelectGrop()
        {
            if (_context.Location == null) return;

            Input.Campuses = await _context.Location.Where(l => l.LocationLevel == 0)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Input.Campuses.Insert(0, new("(未选择)", "0"));

            Input.Libraries = await _context.Location.Where(l => l.LocationLevel == 1 && l.LocationParent == Input.selectGroupView.CampusId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Input.Libraries.Insert(0, new("(未选择)", "0"));

            Input.Floors = await _context.Location.Where(l => l.LocationLevel == 2 && l.LocationParent == Input.selectGroupView.LibraryId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Input.Floors.Insert(0, new("(未选择)", "0"));

            Input.Bookshelves = await _context.Location.Where(l => l.LocationLevel == 3 && l.LocationParent == Input.selectGroupView.FloorId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Input.Bookshelves.Insert(0, new("(未选择)", "0"));

            Input.Layers = await _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == Input.selectGroupView.BookshelfId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Input.Layers.Insert(0, new("(未选择)", "0"));

            Input.IfConflict = new List<SelectListItem>
            {
                new SelectListItem("不插入", "0"),
                new SelectListItem("插入旧值", "1"),
                new SelectListItem("修改图书信息并插入", "2"),
            };
        }
        public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
        {
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
    }
}
