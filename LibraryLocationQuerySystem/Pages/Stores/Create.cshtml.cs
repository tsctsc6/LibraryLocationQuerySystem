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
using LibraryLocationQuerySystem.Utilities;

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
            public List<SelectListItem>? Campuses { get; set; }
            public List<SelectListItem>? Libraries { get; set; }
            public List<SelectListItem>? Floors { get; set; }
            public List<SelectListItem>? Bookshelves { get; set; }
            public List<SelectListItem>? Layers { get; set; }
            public List<SelectListItem>? IfConflict { get; set; }
        }
        public class SelectGroupViewModel
        {
            public int CampusId { get; set; }
            public int LibraryId { get; set; }
            public int FloorId { get; set; }
            public int BookshelfId { get; set; }
            public int LayerId { get; set; }
            public int IfConflict { get; set; }
        }

        [BindProperty]
        public Store Store { get; set; } = default!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await InitSelectGrop();
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            await InitSelectGrop();
            //PrintModelState.printErrorMessage(ModelState);
            if (!ModelState.IsValid || _context.Store == null || Store == null ||
                _context.Book == null || _context.Location == null || Store.Book == null)
            {
                ModelState.AddModelError(string.Empty, "Error occur");
                return Page();
            }

            var loc = await GetLocation();
            if (loc == null)
            {
                ModelState.AddModelError(string.Empty, "请输入完整地址");
                return Page();
            }
            Store.Book.ManageBy = User?.Identity?.Name;
            var oldBook = await GetBook();
            if (oldBook == null) _context.Book.Add(Store.Book);
            else
            {
                switch(Input.selectGroupView.IfConflict)
                {
                    case 0:
                        ModelState.AddModelError(string.Empty, "中图法分类号和书次号冲突，不更新");
                        return Page();
                    case 1: Store.Book = oldBook; break;
                    case 2: _context.Attach(Store.Book).State = EntityState.Modified; break;
                }
            }

            Store.Location = loc;
            Store.RemainNum = Store.StoreNum;
            Store.ManageBy = User?.Identity?.Name;
            _context.Store.Add(Store);
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException e)
            {
                ModelState.AddModelError(string.Empty, e.InnerException?.Message??e.Message);
                return Page();
            }

            return RedirectToPage("./Index");
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

        private async Task<Location?> GetLocation()
        {
            if (_context.Location == null || Input.selectGroupView.CampusId == 0 ||
                Input.selectGroupView.LibraryId == 0 || Input.selectGroupView.FloorId == 0 ||
                Input.selectGroupView.BookshelfId == 0 || Input.selectGroupView.LayerId == 0) return null;
            var loc = await _context.Location.Where(l => l.LocationLevel == 4 &&
                l.LocationId == Input.selectGroupView.LayerId).FirstOrDefaultAsync();
            return loc;
        }

        private async Task<Book?> GetBook()
        {
            if (_context.Book == null || Store.Book == null) return null;
            var b = await _context.Book.Where(b => b.BookSortCallNumber == Store.Book.BookSortCallNumber && b.BookFormCallNumber == Store.Book.BookFormCallNumber)
                .FirstOrDefaultAsync();
            return b;
        }

        public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
        {
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
    }
}
