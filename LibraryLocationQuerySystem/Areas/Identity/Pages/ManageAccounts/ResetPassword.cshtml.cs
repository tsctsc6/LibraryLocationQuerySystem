using LibraryLocationQuerySystem.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryLocationQuerySystem.Areas.Identity.Pages.ManageAccounts
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<StudentUser> _userManager;

        public ResetPasswordModel(UserManager<StudentUser> userManager)
        {
            _userManager = userManager;
        }

        public List<string> ResultMessages { get; private set; } = new();

        private string defaultPassword = "Abc@123";

        public async Task<IActionResult> OnGetAsync(string? StudentId)
        {
            return NotFound();
        }
        public async Task<IActionResult> OnPostAsync(string? StudentId)
        {
            if (StudentId == null) return NotFound();
            var user = await _userManager.FindByNameAsync(StudentId);
            if (user == null)
            {
                ResultMessages.Add("no user");
                return Page();
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("admin"))
            {
                return Redirect("../Account/AccessDenied");
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, code, defaultPassword);
            if (result.Succeeded)
            {
                user.IsPasswordChanged = false;
                ResultMessages.Add($"{StudentId} ÷ÿ÷√√‹¬Î≥…π¶");
                return Page();
            }

            foreach (var error in result.Errors)
            {
                ResultMessages.Add(error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
