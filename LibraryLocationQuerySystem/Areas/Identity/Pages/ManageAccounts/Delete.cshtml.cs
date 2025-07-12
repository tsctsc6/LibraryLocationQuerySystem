using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryLocationQuerySystem.Areas.Identity.Pages.ManageAccounts
{
	public class DeleteModel : PageModel
    {
		private readonly UserManager<StudentUser> _userManager;

        public DeleteModel(UserManager<StudentUser> userManager)
        {
            _userManager = userManager;

		}

		public StudentUser? User { get; private set; } = null;

		public async Task<IActionResult> OnGetAsync(string? StudentId)
        {
			if (StudentId == null) return NotFound();
			User = await _userManager.FindByNameAsync(StudentId);
			if (User == null) return NotFound();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync(string? StudentId)
		{
			if (StudentId == null) return NotFound();
			var user = await _userManager.FindByNameAsync(StudentId);
			if (user == null) return RedirectToPage("./Index");
			var result = await _userManager.DeleteAsync(user);
			return RedirectToPage("./Index");
		}
	}
}
