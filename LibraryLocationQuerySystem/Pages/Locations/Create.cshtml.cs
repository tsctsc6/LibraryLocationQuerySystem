using Humanizer.Bytes;
using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Migrations;
using LibraryLocationQuerySystem.Models;
using LibraryLocationQuerySystem.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibraryLocationQuerySystem.Pages.Locations
{
    public class CreateModel : PageModel
    {
        public class LocationNames
        {
            [StringLength(30)]
            [Display(Name = "校区名")]
            public string? CampusName { get; set; }
            [StringLength(30)]
            [Display(Name = "图书馆/图书室名")]
            public string? LibraryName { get; set; }
            [StringLength(30)]
            [Display(Name = "楼层名")]
            public string? FloorName { get; set; }
            [StringLength(30)]
            [Display(Name = "书架名")]
            public string? BookshelfName { get; set; }
            [StringLength(30)]
            [Display(Name = "书架层名")]
            public string? LayerName { get; set; }
        }

        public class SelectGroupView
        {
			public int CampusId { get; set; }
			public int LibraryId { get; set; }
			public int FloorId { get; set; }
			public int BookshelfId { get; set; }
            //public int LayerId { get; set; }
        }

        private readonly StoreManagerDbContext _context;

		[BindProperty(SupportsGet = true)]
		public SelectGroupView selectGroupView { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public LocationNames locationNames { get; set; } = new();
        public List<SelectListItem> Campuses { get; set; }
		public List<SelectListItem> Libraries { get; set; }
		public List<SelectListItem> Floors { get; set; }
		public List<SelectListItem> Bookshelves { get; set; }
        //public List<SelectListItem> Layers { get; set; }

        public Location[] Locations { get; } = new Location[5];

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
            await InitSelectGrop();
            //PrintModelState.printErrorMessage(ModelState);
            if (_context.Location == null)
            {
                return Page();
            }

            (int,int) index;
            try { index = await SetLocationLevelAndParent(); }
            catch (ArgumentNullException e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return Page();
            }
            while(index.Item1 < index.Item2)
            {
                do { Locations[index.Item1].GenLocationid(); }
                while (await _context.Location.Where(l => l.LocationId == Locations[index.Item1].LocationId).CountAsync() != 0 || Locations[index.Item1].LocationId == 0);
                if (index.Item1 > 0) Locations[index.Item1].LocationParent = Locations[index.Item1 - 1].LocationId;
                Locations[index.Item1].ManageBy = User?.Identity?.Name;
                await _context.Location.AddAsync(Locations[index.Item1]);
                index.Item1++;
            }

			try { await _context.SaveChangesAsync(); }
			catch (DbUpdateException e)
			{
				ModelState.AddModelError(string.Empty, e.InnerException?.Message ?? string.Empty);
				return Page();
			}
            
            return RedirectToPage("./Index");
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
            /*
            Layers = await _context.Location.Where(l => l.LocationLevel == 4 && l.LocationParent == selectGroupView.BookshelfId)
                .Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString())).ToListAsync();
            Layers.Insert(0, new("(未选择)", "0"));
            */
        }
        
        private async Task<(int, int)> SetLocationLevelAndParent()
        {
            int start = -1;
            if (selectGroupView.CampusId != 0)
            {
                Locations[0] = await _context.Location.SingleAsync(l => l.LocationLevel == 0 && l.LocationId == selectGroupView.CampusId);
            }
            else if (locationNames.CampusName != null)
            {
                Locations[0] = new();
                Locations[0].LocationLevel = 0;
                Locations[0].LocationParent = 0;
                Locations[0].LocationName = locationNames.CampusName;
                if (start == -1) start = 0;
            }
            else throw new ArgumentNullException("locationNames.CampusName == null");
            if (selectGroupView.LibraryId != 0)
            {
                Locations[1] = await _context.Location.SingleAsync(l => l.LocationLevel == 1 && l.LocationId == selectGroupView.LibraryId);
            }
            else if (locationNames.LibraryName != null)
            {
                Locations[1] = new();
                Locations[1].LocationLevel = 1;
                Locations[1].LocationParent = selectGroupView.CampusId;
                //if (locationNames.LibraryName == null) throw new ArgumentNullException("locationNames.LibraryName == null");
                Locations[1].LocationName = locationNames.LibraryName;
                if (start == -1) start = 1;
            }
            else return (start, 1);
            if (selectGroupView.FloorId != 0)
            {
                Locations[2] = await _context.Location.SingleAsync(l => l.LocationLevel == 2 && l.LocationId == selectGroupView.FloorId);
            }
            else if (locationNames.FloorName != null)
            {
                Locations[2] = new();
                Locations[2].LocationLevel = 2;
                Locations[2].LocationParent = selectGroupView.LibraryId;
                //if (locationNames.FloorName == null) throw new ArgumentNullException("locationNames.FloorName == null");
                Locations[2].LocationName = locationNames.FloorName;
                if (start == -1) start = 2;
            }
            else return (start, 2);
            if (selectGroupView.BookshelfId != 0)
            {
                Locations[3] = await _context.Location.SingleAsync(l => l.LocationLevel == 3 && l.LocationId == selectGroupView.BookshelfId);
            }
            else if (locationNames.BookshelfName != null)
            {
                Locations[3] = new();
                Locations[3].LocationLevel = 3;
                Locations[3].LocationParent = selectGroupView.FloorId;
                //if (locationNames.BookshelfName == null) throw new ArgumentNullException("locationNames.BookshelfName == null");
                Locations[3].LocationName = locationNames.BookshelfName;
                if (start == -1) start = 3;
            }
            else return (start, 3);

            Locations[4] = new();
            Locations[4].LocationLevel = 4;
            Locations[4].LocationParent = selectGroupView.BookshelfId;
            if (locationNames.LayerName == null) return (start, 4);
            Locations[4].LocationName = locationNames.LayerName;
            if (start == -1) start = 4;
            return (start, 5);
        }
        
        public async Task<JsonResult> OnGetParentAsync(int LocationLevel, int LocationParent)
		{
            if (_context.Location == null) return new JsonResult("_context.Location == null");
            var res = await _context.Location.Where(l => l.LocationLevel == LocationLevel && l.LocationParent == LocationParent).ToListAsync();
            return new JsonResult(res);
        }
		
    }
}
