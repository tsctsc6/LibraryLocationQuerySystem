using Humanizer.Bytes;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Migrations;
using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibraryLocationQuerySystem.Pages.Locations
{
    public class CreateModel : PageModel
    {
		public class SelectGroupView
        {
			public byte CampusId { get; set; }
			public byte LibraryId { get; set; }
			public byte FloorId { get; set; }
			public byte BookshelfId { get; set; }
			//public byte LayerId { get; set; }
		}

		private readonly StoreManagerDbContext _context;

		[BindProperty(SupportsGet = true)]
		public SelectGroupView selectGroupView { get; set; } = new();
		public List<SelectListItem> Campuses { get; set; }
		public List<SelectListItem> Libraries { get; set; }
		public List<SelectListItem> Floors { get; set; }
		public List<SelectListItem> Bookshelves { get; set; }
		//public List<SelectListItem> Layers { get; set; }

        [BindProperty]
        public Location Location { get; set; } = default!;

        public CreateModel(StoreManagerDbContext context)
		{
			_context = context;
		}

		public async Task OnGetAsync()
		{
			await InitSelectGrop();
        }

        public async Task<IActionResult> OnPostAsync()
		{
            if (!ModelState.IsValid || _context.Location == null || Location == null)
            {
                return Page();
            }
            SetLocationLevelAndParent();
            await _context.Location.AddAsync(Location);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task InitSelectGrop()
		{
            if (_context.Location == null) return;

            Campuses = await _context.Location.Where(l => l.LocationLevel == 0)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Campuses.Insert(0, new("(Î´Ñ¡Ôñ)", "0"));

            Libraries = await _context.Location.Where(l => l.LocationLevel == 1 && l.LocationParent == selectGroupView.CampusId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Libraries.Insert(0, new("(Î´Ñ¡Ôñ)", "0"));

            Floors = await _context.Location.Where(l => l.LocationLevel == 2 && l.LocationParent == selectGroupView.LibraryId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Floors.Insert(0, new("(Î´Ñ¡Ôñ)", "0"));

            Bookshelves = await _context.Location.Where(l => l.LocationLevel == 3 && l.LocationParent == selectGroupView.FloorId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Bookshelves.Insert(0, new("(Î´Ñ¡Ôñ)", "0"));
            /*
            Layers = await _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == selectGroupView.BookshelfId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Layers.Insert(0, new("(Î´Ñ¡Ôñ)", "0"));
            */
        }

        private void SetLocationLevelAndParent()
        {
            if (selectGroupView.CampusId == 0)
            {
                Location.LocationLevel = 0;
                Location.LocationParent = 0;
                return;
            }
            if (selectGroupView.LibraryId == 0)
            {
                Location.LocationLevel = 1;
                Location.LocationParent = selectGroupView.CampusId;
                return;
            }
            if (selectGroupView.FloorId == 0)
            {
                Location.LocationLevel = 2;
                Location.LocationParent = selectGroupView.LibraryId;
                return;
            }
            if (selectGroupView.BookshelfId == 0)
            {
                Location.LocationLevel = 3;
                Location.LocationParent = selectGroupView.FloorId;
                return;
            }
            else
            {
                Location.LocationLevel = 4;
                Location.LocationParent = selectGroupView.BookshelfId;
                return;
            }
        }
        
		public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
		{
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
		
    }
}
