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
using LibraryLocationQuerySystem.Pages.Locations;

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
            var _StoreList = StoreInLocation();
            if (_context.Book == null) return Page();
            var _BookList = from b in _context.Book
                            join s in _StoreList on
                            new { b.BookSortCallNumber, b.BookFormCallNumber } equals
                            new { s.BookSortCallNumber, s.BookFormCallNumber }
                            select b;
            if (!string.IsNullOrEmpty(searchOption.SearchString))
            {
                _BookList = _BookList.Where(b =>
                    (searchOption.SearchBookBookSortCallNumber && b.BookSortCallNumber.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchBookBookFormCallNumber && b.BookFormCallNumber.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchBookName && b.BookName.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchPublishingHouse && b.PublishingHouse.Contains(searchOption.SearchString)) ||
                    (searchOption.SearchBookAuthor && b.Author.Contains(searchOption.SearchString))
                );
            }
            var _StoreList2 = from b in _BookList
                              join s in _StoreList on
                              new { b.BookSortCallNumber, b.BookFormCallNumber } equals
                              new { s.BookSortCallNumber, s.BookFormCallNumber }
                              select s;
            StoreList = await _StoreList2.ToArrayAsync();
            _ = await _BookList.ToArrayAsync();
            return Page();
        }

        private IQueryable<Store>? StoreInLocation()
        {
            if (_context.Location == null || _context.Book == null || _context.Store == null) return null;
            IQueryable<Store>? _StoreList = null;
            if (searchOption.selectGroupView.CampusId == 0)
            {
                _StoreList =  _context.Store;
                return _StoreList;
            }
            IQueryable<Location>? LocationList = null;
            if (searchOption.selectGroupView.LibraryId == 0)
            {
                LocationList = GetEndLocations(0, searchOption.selectGroupView.CampusId);
            }
            else if (searchOption.selectGroupView.FloorId == 0)
            {
                LocationList = GetEndLocations(1, searchOption.selectGroupView.LibraryId);
            }
            else if (searchOption.selectGroupView.BookshelfId == 0)
            {
                LocationList = GetEndLocations(2, searchOption.selectGroupView.FloorId);
            }
            else if (searchOption.selectGroupView.LayerId == 0)
            {
                LocationList = GetEndLocations(3, searchOption.selectGroupView.BookshelfId);
            }
            else
            {
                LocationList = GetEndLocations(4, searchOption.selectGroupView.LayerId);
            }
            if (LocationList == null) return null;
            if (_context.Store == null) return null;
            _StoreList = from l in LocationList
                         join s in _context.Store on
                         new { l.LocationLevel, l.LocationId } equals
                         new { s.LocationLevel, s.LocationId }
                         select s;
            return _StoreList;
        }
        private IQueryable<Location>? GetEndLocations(byte level, int id)
        {
            if (_context.Location == null) return null;
            IQueryable<Location>? _loc = null;
            IQueryable<Location>? _loc_1 = null;
            IQueryable<Location>? _loc_2 = null;
            IQueryable<Location>? _loc_3 = null;
            IQueryable<Location>? _loc_4 = null;
            switch (level)
            {
                case 0:
                    _loc_1 = _context.Location.Where(l => l.LocationLevel == 1 && l.LocationParent == id);
                    _loc = _context.Location.Where(l => l.LocationLevel == 2);
                    _loc_2 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    _loc = _context.Location.Where(l => l.LocationLevel == 3);
                    _loc_3 = from l in _loc
                             join l2 in _loc_2 on l.LocationParent equals l2.LocationId
                             select l;
                    _loc = _context.Location.Where(l => l.LocationLevel == 4);
                    _loc_4 = from l in _loc
                             join l3 in _loc_3 on l.LocationParent equals l3.LocationId
                             select l;
                    return _loc_4;
                case 1:
                    _loc_2 = _context.Location.Where(l => l.LocationLevel == 2 && l.LocationParent == id);
                    _loc = _context.Location.Where(l => l.LocationLevel == 3);
                    _loc_3 = from l in _loc
                             join l2 in _loc_2 on l.LocationParent equals l2.LocationId
                             select l;
                    _loc = _context.Location.Where(l => l.LocationLevel == 4);
                    _loc_4 = from l in _loc
                             join l3 in _loc_3 on l.LocationParent equals l3.LocationId
                             select l;
                    return _loc_4;
                case 2: 
                    _loc_3 = _context.Location.Where(l => l.LocationLevel == 3 && l.LocationParent == id);
                    _loc = _context.Location.Where(l => l.LocationLevel == 4);
                    _loc_4 = from l in _loc
                             join l3 in _loc_3 on l.LocationParent equals l3.LocationId
                             select l;
                    return _loc_4;
                case 3: _loc_4 = _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == id); return _loc_4;
                case 4: _loc_4 = _context.Location.Where(l => l.LocationLevel == 4 && l.LocationId == id); return _loc_4;
                default: return _loc_4;
            }
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
