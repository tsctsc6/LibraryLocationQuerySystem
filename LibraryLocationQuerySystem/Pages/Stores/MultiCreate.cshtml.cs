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
            if (_context.Store == null)
            {
                ModelState.AddModelError(string.Empty, "_context.Store == null");
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
            var loc = GetLocation();
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
                    var cs = worksheet.Cells[2, 1, maxRow, 8];
                    foreach(var c in cs)
                    {
                        Console.WriteLine(c.Value.ToString());
                    }
                }
            }
            /*
            _context.Store.Add(Store);
            await _context.SaveChangesAsync();
            */
            return Page();
            //return RedirectToPage("./Index");
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
