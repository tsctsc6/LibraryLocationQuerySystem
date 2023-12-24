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
using LibraryLocationQuerySystem.Utilities;
using OfficeOpenXml;
using Microsoft.IdentityModel.Tokens;
using Humanizer.Bytes;
using static LibraryLocationQuerySystem.Pages.Locations.CreateModel;

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
            public int IfConflict { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public SelectGroupViewModel selectGroupView { get; set; } = new();
        public List<SelectListItem> Campuses { get; set; }
        public List<SelectListItem> Libraries { get; set; }
        public List<SelectListItem> Floors { get; set; }
        public List<SelectListItem> Bookshelves { get; set; }
        public List<SelectListItem> Layers { get; set; }
        public List<SelectListItem> IfConflict { get; set; }

        [BindProperty]
        public BufferedSingleFileUploadPhysical FileUpload { get; set; }
        private readonly string[] _permittedExtensions = { ".xlsx" };
        private readonly long _fileSizeLimit = 2097152;

        public async Task<IActionResult> OnGet()
        {
            await InitSelectGrop();
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            await InitSelectGrop();
            if (_context.Store == null || _context.Book == null)
            {
                ModelState.AddModelError(string.Empty, "_context.Store == null || _context.Book == null");
                return Page();
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please correct the form.");
                return Page();
            }
            var formFileContent =
                await FileHelpers.ProcessFormFile<BufferedSingleFileUploadPhysical>(
                    FileUpload.FormFile, ModelState, _permittedExtensions, _fileSizeLimit);
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please correct the form.");
                return Page();
            }
            var loc = await GetLocation();
            if (loc == null)
            {
                ModelState.AddModelError(string.Empty, "请选择具体的地点");
                return Page();
            }
            using (var fileStream = new MemoryStream(formFileContent))
            {
                using (var package = new ExcelPackage(fileStream))
                {

                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ModelState.AddModelError(string.Empty, "这个文件没有Worksheet");
                        return Page();
                    }
                    var maxAddress = worksheet.Dimension.Address.Split(":");
                    int maxRow = (int)OpenXmlHelper.AddressSplitRow(maxAddress[1]);
                    for (int i = 2; i <= maxRow; i++)
                    {
                        Store s = new();
                        s.Book = new();
                        Book? oldBook = null;
                        s.Location = loc;

                        try
                        {
                            s.Book.BookSortCallNumber = worksheet.Cells[i, 5]?.Value?.ToString()?.Split('/')[0];
                            s.Book.BookFormCallNumber = worksheet.Cells[i, 5]?.Value?.ToString()?.Split('/')[1];
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 5]处格式错误: {e.Message}");
                            continue;
                        }
                        if (s.Book.BookSortCallNumber == null || s.Book.BookFormCallNumber == null)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 5]处格式错误: null");
                            continue;
                        }


                        if (await _context.Store.Where(
                            ss => ss.BookSortCallNumber == s.Book.BookSortCallNumber &&
                            ss.BookFormCallNumber == s.Book.BookFormCallNumber &&
                            ss.LocationLevel == s.Location.LocationLevel &&
                            ss.LocationId == s.Location.LocationId).CountAsync() != 0)
                        {
                            ModelState.AddModelError(string.Empty, $"第{i}行，该书已存在，不更新");
                            if (selectGroupView.IfConflict != 2) continue;
                        }
                        oldBook = await GetBook(s.Book.BookSortCallNumber, s.Book.BookFormCallNumber);
                        if (oldBook != null)
                        {
                            switch (selectGroupView.IfConflict)
                            {
                                case 0:
                                    ModelState.AddModelError(string.Empty, $"第{i}行，中图法分类号和书次号冲突，不更新");
                                    continue;
                                case 1:
                                    s.Book = oldBook;
                                    goto A;
                                case 2: break;
                                default: break;
                            }
                        }

                        try
                        {
                            s.Book.BookName = worksheet.Cells[i, 1]?.Value?.ToString();
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 1]处格式错误: {e.Message}");
                            continue;
                        }
                        if (s.Book.BookName == null)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 1]处格式错误: null");
                            continue;
                        }

                        try
                        {
                            s.Book.Author = worksheet.Cells[i, 2]?.Value?.ToString();
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 2]处格式错误: {e.Message}");
                            continue;
                        }
                        if (s.Book.Author == null)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 2]处格式错误: null");
                            continue;
                        }

                        try
                        {
                            s.Book.PublishingHouse = worksheet.Cells[i, 3]?.Value?.ToString();
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 3]处格式错误: {e.Message}");
                            continue;
                        }
                        if (s.Book.PublishingHouse == null)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 3]处格式错误: null");
                            continue;
                        }

                        try
                        {
                            s.Book.PublicDate = DateTime.Parse(worksheet.Cells[i, 4]?.Value?.ToString());
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 4]处格式错误: {e.Message}");
                            continue;
                        }

                        try
                        {
                            string? ts = worksheet.Cells[i, 6]?.Value?.ToString();
                            if (ts.IsNullOrEmpty()) throw new ArgumentNullException($"null");
                            switch(ts)
                            {
                                case "图书": s.Book.Type = BookType.图书; break;
                                case "期刊": s.Book.Type = BookType.期刊; break;
                                case "报纸": s.Book.Type = BookType.报纸; break;
                                case "附书光盘": s.Book.Type = BookType.附书光盘; break;
                                case "非书资料": s.Book.Type = BookType.非书资料; break;
                                default: throw new Exception("请输入\"图书、期刊、报纸、附书光盘、非书资料\"之一");
                            }
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 6]处格式错误: {e.Message}");
                            continue;
                        }

                        try
                        {
                            s.Book.EndDate = DateTime.Parse(worksheet.Cells[i, 8]?.Value?.ToString());
                        }
                        catch (Exception)
                        {
                            s.Book.EndDate = null;
                        }

                        if (selectGroupView.IfConflict == 2) _context.Attach(s.Book).State = EntityState.Modified;
                    A:
                        try
                        {
                            s.StoreNum = byte.Parse(worksheet.Cells[i, 7]?.Value?.ToString());
                            s.RemainNum = s.StoreNum;
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(string.Empty, $"[{i}, 7]处格式错误: {e.Message}");
                            continue;
                        }

                        _context.Store.Add(s);
                    }
                    try { await _context.SaveChangesAsync(); }
                    catch (DbUpdateException e)
                    {
                        ModelState.AddModelError(string.Empty, e.InnerException?.Message ?? e.Message);
                        return Page();
                    }
                }
            }
            return Page();
        }

        private async Task<Location?> GetLocation()
        {
            if (_context.Location == null || selectGroupView.CampusId == 0 ||
                selectGroupView.LibraryId == 0 || selectGroupView.FloorId == 0 ||
                selectGroupView.BookshelfId == 0 || selectGroupView.LayerId == 0) return null;
            var loc = await _context.Location.Where(l => l.LocationLevel == 4 &&
                l.LocationId == selectGroupView.LayerId).FirstOrDefaultAsync();
            return loc;
        }

        private async Task<Book?> GetBook(string scn, string fcn)
        {
            if (_context.Book == null) return null;
            var b = await _context.Book.Where(b => b.BookSortCallNumber == scn && b.BookFormCallNumber == fcn)
                .FirstOrDefaultAsync();
            return b;
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

            IfConflict = new List<SelectListItem>
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
