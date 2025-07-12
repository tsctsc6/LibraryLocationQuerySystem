using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryLocationQuerySystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<StudentUser> _userManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(UserManager<StudentUser> userManager,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public bool IsPasswordChanged { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Page();
            IsPasswordChanged = user.IsPasswordChanged;
            return Page();
        }
    }
}