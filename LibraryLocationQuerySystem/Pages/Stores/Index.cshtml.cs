using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using static LibraryLocationQuerySystem.Pages.Locations.CreateModel;
using LibraryLocationQuerySystem.Utilities;
using System.ComponentModel.DataAnnotations;
using static LibraryLocationQuerySystem.Pages.Stores.IndexModel;

namespace LibraryLocationQuerySystem.Pages.Stores
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public IndexModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
        }

        [BindProperties(SupportsGet = true)]
        public class SearchOption
        {
            public string SearchString;

            [DisplayName("搜索中图法分类号")]
            public bool SearchBookSortCallNumber { get; set; }

            [DisplayName("搜索书次号")]
            public bool SearchBookFormCallNumber { get; set; }

            [DisplayName("搜索书名")]
            public bool SearchBookName { get; set; } = true;

            [DisplayName("搜索出版社")]
            public bool SearchPublishingHouse { get; set; }

            [DisplayName("搜索作者")]
            public bool SearchBookAuthor { get; set; }

            public SelectGroupView selectGroupView { get; set; } = new();
            public List<SelectListItem> Campuses { get; set; }
            public List<SelectListItem> Libraries { get; set; }
            public List<SelectListItem> Floors { get; set; }
            public List<SelectListItem> Bookshelves { get; set; }
            public List<SelectListItem> Layers { get; set; }
        }

        public class SelectGroupView
        {
            public short CampusId { get; set; }
            public short LibraryId { get; set; }
            public short FloorId { get; set; }
            public short BookshelfId { get; set; }
            public short LayerId { get; set; }
        }

        public IList<Store> Store { get;set; } = default!;

        public SearchOption searchOption { get; set; } = new();

        public PageManager pm { get; set; } = new() { NumPerPage = 20 };
        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public int pageNum { get; set; } = 0;

        public async Task OnGetAsync()
        {
            await InitSelectGrop();
            Store = new List<Store>();
        }

        private async Task InitSelectGrop()
        {
            if (_context.Location == null) return;

            searchOption.Campuses = await _context.Location.Where(l => l.LocationLevel == 0)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            searchOption.Campuses.Insert(0, new("(未选择)", "0"));

            searchOption.Libraries = await _context.Location.Where(l => l.LocationLevel == 1 && l.LocationParent == searchOption.selectGroupView.CampusId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            searchOption.Libraries.Insert(0, new("(未选择)", "0"));

            searchOption.Floors = await _context.Location.Where(l => l.LocationLevel == 2 && l.LocationParent == searchOption.selectGroupView.LibraryId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            searchOption.Floors.Insert(0, new("(未选择)", "0"));

            searchOption.Bookshelves = await _context.Location.Where(l => l.LocationLevel == 3 && l.LocationParent == searchOption.selectGroupView.FloorId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            searchOption.Bookshelves.Insert(0, new("(未选择)", "0"));

            searchOption.Layers = await _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == searchOption.selectGroupView.BookshelfId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            searchOption.Layers.Insert(0, new("(未选择)", "0"));
        }
        public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
        {
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
    }
}
