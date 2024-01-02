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
using OfficeOpenXml;
using System.Text;

namespace LibraryLocationQuerySystem.Pages.Stores
{
    public class IndexModel : PageModel
    {
        private readonly LibraryLocationQuerySystem.Data.StoreManagerDbContext _context;

        public IndexModel(LibraryLocationQuerySystem.Data.StoreManagerDbContext context)
        {
            _context = context;
            pm = new() { NumPerPage = 20 };
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

        public PageManager pm { get; private set; }
        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public int pageNum { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            await InitSelectGrop();
            var _StoreList = StoreInLocation();
            if (_context.Book == null) return Page();
            await Filter(_StoreList);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitSelectGrop();
            var _StoreList = StoreInLocation();
            if (_context.Book == null) return Page();
            await Filter(_StoreList);
            byte[] bytes;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Test");
                worksheet.Cells[1, 1].Value = "书名";
                worksheet.Cells[1, 2].Value = "作者";
                worksheet.Cells[1, 3].Value = "出版社";
                worksheet.Cells[1, 4].Value = "出版日期";
                worksheet.Cells[1, 5].Value = "索取号";
                worksheet.Cells[1, 6].Value = "类型";
                worksheet.Cells[1, 7].Value = "结束出版日期";
                worksheet.Cells[1, 8].Value = "存储地点";
                int i = 2;
                foreach (var s in StoreList)
                {
                    worksheet.Cells[i, 1].Value = s.Book.BookName;
                    worksheet.Cells[i, 2].Value = s.Book.Author;
                    worksheet.Cells[i, 3].Value = s.Book.PublishingHouse;
                    worksheet.Cells[i, 4].Value = s.Book.PublicDate;
                    worksheet.Cells[i, 4].Style.Numberformat.Format = "yyyy/mm/dd";
                    worksheet.Cells[i, 5].Value = s.Book.BookSortCallNumber.TrimEnd() + "/" + s.Book.BookFormCallNumber.TrimEnd();
                    worksheet.Cells[i, 6].Value = s.Book.Type;
                    worksheet.Cells[i, 7].Value = s.Book.EndDate;
                    worksheet.Cells[i, 7].Style.Numberformat.Format = "yyyy/mm/dd";
                    worksheet.Cells[i, 8].Value = await SetLocationPath(s.LocationLevel, s.LocationId);
                    i++;
                }
                bytes = package.GetAsByteArray();
            }
            Response.Headers.Add("Content-Disposition", "attachment; filename=SearchResult.xlsx");
            return new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        /// <summary>
        /// 搜索，并执行查询结果
        /// </summary>
        /// <param name="_StoreList"></param>
        /// <returns></returns>
        private async Task Filter(IQueryable<Store> _StoreList)
        {
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
            pm.Set(pageNum, await _StoreList2.CountAsync());
            _StoreList2 = _StoreList2.Skip(pm.StartIndex).Take(pm.NumPerPage);
            StoreList = await _StoreList2.ToArrayAsync();
            _BookList = from b in _BookList
                        join s in _StoreList2 on
                        new { b.BookSortCallNumber, b.BookFormCallNumber } equals
                        new { s.BookSortCallNumber, s.BookFormCallNumber }
                        select b;
            _ = await _BookList.ToListAsync();
        }
        /// <summary>
        /// 根据Locations，找到对应的Stores
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 查找全部子节点的LocationId
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns>查询对象</returns>
        private IQueryable<Location>? GetEndLocations(byte level, int id)
        {
            if (_context.Location == null) return null;
            IQueryable<Location>? _loc = null;
            IQueryable<Location>? _loc_1 = null;
            /*
            if (level < 3)
            {
                _loc_1 = _context.Location.Where(l => l.LocationLevel == level + 1 && l.LocationParent == id);
                Console.WriteLine($"MyGO!!!!!{level}, {_loc_1.Count()}");
                while (level < 3)
                {
                    _loc = _context.Location.Where(l => l.LocationLevel == level + 2);
                    Console.WriteLine($"BanG Dream {level}, {_loc.Count()}");
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    Console.WriteLine($"MyGO!!!!!{level}, {_loc_1.Count()}");
                    level++;
                }
                Console.WriteLine($"MyGO!!!!!{level}, {_loc_1.Count()}");
                return _loc_1;
            }
            if (level == 3)
            {
                _loc_1 = _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == id);
                return _loc_1;
            }
            if (level == 4)
            {
                _loc_1 = _context.Location.Where(l => l.LocationLevel == 4 && l.LocationId == id);
                return _loc_1;
            }
            return _loc_1;
            */
            
            switch (level)
            {
                case 0:
                    _loc_1 = _context.Location.Where(l => l.LocationLevel == 1 && l.LocationParent == id);
                    _loc = _context.Location.Where(l => l.LocationLevel == 2);
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    _loc = _context.Location.Where(l => l.LocationLevel == 3);
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    _loc = _context.Location.Where(l => l.LocationLevel == 4);
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    return _loc_1;
                case 1:
                    _loc_1 = _context.Location.Where(l => l.LocationLevel == 2 && l.LocationParent == id);
                    _loc = _context.Location.Where(l => l.LocationLevel == 3);
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    _loc = _context.Location.Where(l => l.LocationLevel == 4);
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    return _loc_1;
                case 2: 
                    _loc_1 = _context.Location.Where(l => l.LocationLevel == 3 && l.LocationParent == id);
                    _loc = _context.Location.Where(l => l.LocationLevel == 4);
                    _loc_1 = from l in _loc
                             join l1 in _loc_1 on l.LocationParent equals l1.LocationId
                             select l;
                    return _loc_1;
                case 3: _loc_1 = _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == id); return _loc_1;
                case 4: _loc_1 = _context.Location.Where(l => l.LocationLevel == 4 && l.LocationId == id); return _loc_1;
                default: return _loc_1;
            }
            
        }
        /// <summary>
        /// 根据Location信息，生成路径信息
        /// </summary>
        /// <param name="LocationLevel"></param>
        /// <param name="LocationId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<string> SetLocationPath(byte LocationLevel, int LocationId)
        {
            if (_context.Location == null) return string.Empty;
            List<string> strings = new();
            while (LocationLevel >= 0 && LocationLevel < 5)
            {
                var loc = await _context.Location.Where(l => l.LocationLevel == LocationLevel &&
                    l.LocationId == LocationId).FirstOrDefaultAsync();
                if (loc == null) throw new ArgumentNullException("Location元组not find");
                LocationId = loc.LocationParent;
                LocationLevel--;
                strings.Insert(0, loc.LocationName);
            }
            StringBuilder sb = new("/ ");
            foreach (var item in strings)
            {
                sb.Append(item);
                sb.Append(" / ");
            }
            return sb.ToString();
        }
        /// <summary>
        /// 初始化5个位置选择框和策略处理选择框
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 查询Location下一级的子节点信息
        /// </summary>
        /// <param name="LocationLevel"></param>
        /// <param name="LocationParent"></param>
        /// <returns>json字符串</returns>
        public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
        {
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
    }
}
