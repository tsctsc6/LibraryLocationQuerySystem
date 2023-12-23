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

        public class SearchOptionModel
        {
            public string SearchString { get; set; }

            [DisplayName("搜索中图法分类号")]
            public bool SearchBookBookSortCallNumber { get; set; }

            [DisplayName("搜索书次号")]
            public bool SearchBookBookFormCallNumber { get; set; }

            [DisplayName("搜索书名")]
            public bool SearchBookName { get; set; } = true;

            [DisplayName("搜索出版社")]
            public bool SearchPublishingHouse { get; set; }

            [DisplayName("搜索作者")]
            public bool SearchBookAuthor { get; set; }

            [BindProperty(SupportsGet = true)]
            public SelectGroupViewModel selectGroupView { get; set; } = new();
            public List<SelectListItem> Campuses { get; set; }
            public List<SelectListItem> Libraries { get; set; }
            public List<SelectListItem> Floors { get; set; }
            public List<SelectListItem> Bookshelves { get; set; }
            public List<SelectListItem> Layers { get; set; }
        }

        public class SelectGroupViewModel
        {
            public int CampusId { get; set; }
            public int LibraryId { get; set; }
            public int FloorId { get; set; }
            public int BookshelfId { get; set; }
            public int LayerId { get; set; }
        }

        public IList<Store> StoreList { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public SearchOptionModel searchOption { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public PageManager pm { get; set; } = new() { NumPerPage = 20 };
        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public int pageNum { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            await InitSelectGrop();
            await StoreInLocation();
            if (_context.Book == null) return Page();
            foreach(var item in StoreList)
            {
                _ = await _context.Book.Where(b => b.BookSortCallNumber == item.BookSortCallNumber &&
                    b.BookFormCallNumber == item.BookFormCallNumber).FirstOrDefaultAsync();
            }
            if (!string.IsNullOrEmpty(searchOption.SearchString))
            {
                StoreList = StoreList.Where(b =>
                    (searchOption.SearchBookBookSortCallNumber && b.BookSortCallNumber.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchBookBookFormCallNumber && b.BookFormCallNumber.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchBookName && b.Book.BookName.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchPublishingHouse && b.Book.PublishingHouse.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchBookAuthor && b.Book.Author.Contains(searchOption.SearchString))
                ).ToList();
            }
            return Page();
        }

        private async Task StoreInLocation()
        {
            if (_context.Location == null || _context.Book == null || _context.Store == null) return;
            if (searchOption.selectGroupView.CampusId == 0)
            {
                StoreList = await _context.Store.ToListAsync();
                return;
            }
            List<Location> LocationList = new();
            if (searchOption.selectGroupView.LibraryId == 0)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == 0 &&
                    l.LocationId == searchOption.selectGroupView.CampusId).FirstOrDefaultAsync();
                LocationList = await GetEndLocations(loc);
            }
            else if (searchOption.selectGroupView.FloorId == 0)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == 1 &&
                    l.LocationId == searchOption.selectGroupView.LibraryId).FirstOrDefaultAsync();
                LocationList = await GetEndLocations(loc);
            }
            else if (searchOption.selectGroupView.BookshelfId == 0)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == 2 &&
                    l.LocationId == searchOption.selectGroupView.FloorId).FirstOrDefaultAsync();
                LocationList = await GetEndLocations(loc);
            }
            else if (searchOption.selectGroupView.LayerId == 0)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == 3 &&
                    l.LocationId == searchOption.selectGroupView.BookshelfId).FirstOrDefaultAsync();
                LocationList = await GetEndLocations(loc);
            }
            else
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == 4 &&
                    l.LocationId == searchOption.selectGroupView.LayerId).FirstOrDefaultAsync();
                LocationList = await GetEndLocations(loc);
            }
            StoreList = new List<Store>();
            foreach (var item in LocationList)
            {
                if (_context.Store == null) throw new ArgumentNullException("_context.Store == null");
                _ = await _context.Store.Where(s => s.LocationLevel == item.LocationLevel &&
                        s.LocationId == item.LocationId).ToListAsync();
                StoreList = StoreList.Concat(item.Stores).ToList();
            }
        }

        private async Task<List<Location>> GetEndLocations(Location? location)
        {
            List<Location> list = new();
            if (location == null) return list;
            if (location.LocationLevel == 4)
            {
                list.Add(location);
                return list;
            }
            if (_context.Location == null) return list;
            var _list = await _context.Location.Where(l => l.LocationLevel == location.LocationLevel + 1 &&
                l.LocationParent == location.LocationId).ToListAsync();
            foreach (var item in _list)
            {
                list = list.Concat(await GetEndLocations(item)).ToList();
            }
            return list;
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
