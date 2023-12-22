using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using LibraryLocationQuerySystem.Areas.Identity.Data;
using LibraryLocationQuerySystem.Areas.Identity.Models;
using LibraryLocationQuerySystem.Utilities;

namespace LibraryLocationQuerySystem.Areas.Identity.Pages.ManageAccounts
{
	public class IndexModel : PageModel
	{
		private readonly StudentUserDbContext _context;

        public PageManager pm { get; private set; }
        [BindProperty(SupportsGet = true)]
        [Range(0, int.MaxValue)]
        public int pageNum { get; set; } = 0;

        public IndexModel(StudentUserDbContext context)
		{
			_context = context;
			pm = new() { NumPerPage = 20 };
		}

		public IList<StudentUser> StudentUsers { get; set; } = default!;

		[BindProperty(SupportsGet = true)]
		[RegularExpression(@"\d+")]
		[StringLength(10)]
		public string? SearchString { get; set; }

		public async Task OnGetAsync()
		{
			var Role_admin = _context.Roles.Where(r => r.NormalizedName == "ADMIN").FirstOrDefault();
			if (Role_admin == null) throw new ArgumentNullException("Null ADMIN");
			var StudentUserIds_reader = _context.UserRoles.Where(ur => ur.RoleId == Role_admin.Id);

			IQueryable<StudentUser> _StudentUsers = _context.Users.Except(
				from u in _context.Users
				join ur in StudentUserIds_reader on u.Id equals ur.UserId
				select u);

			if (!string.IsNullOrEmpty(SearchString))
			{
				_StudentUsers = _StudentUsers.Where(s => s.StudentId.Contains(SearchString));
			}
			pm.Set(pageNum, await _StudentUsers.CountAsync());
			StudentUsers = await _StudentUsers.Skip(pm.StartIndex).Take(pm.NumPerPage).ToListAsync();
		}
	}
}
