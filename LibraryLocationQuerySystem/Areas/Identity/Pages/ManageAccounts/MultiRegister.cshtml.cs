using LibraryLocationQuerySystem.Models;
using LibraryLocationQuerySystem.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;

namespace LibraryLocationQuerySystem.Areas.Identity.Pages.ManageAccounts
{
	public class MultiRegisterModel : PageModel
    {
        private readonly SignInManager<StudentUser> _signInManager;
        private readonly UserManager<StudentUser> _userManager;
        private readonly IUserStore<StudentUser> _userStore;
        private readonly ILogger<MultiRegisterModel> _logger;

        public MultiRegisterModel(
            UserManager<StudentUser> userManager,
            IUserStore<StudentUser> userStore,
            SignInManager<StudentUser> signInManager,
            ILogger<MultiRegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
        }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [BindProperty]
        public BufferedSingleFileUploadPhysical FileUpload { get; set; }

        private readonly string[] _permittedExtensions = { ".xlsx" };
        private readonly long _fileSizeLimit = 2097152;
        private string defaultPassword = "Abc@123";

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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
            IEnumerable<string?> studentIds;
            using (var fileStream = new MemoryStream(formFileContent))
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ModelState.AddModelError(string.Empty, "����ļ�û��Worksheet");
                        return Page();
                    }
                    var maxAddress = worksheet.Dimension.Address.Split(":");
                    int maxRow = (int)OpenXmlHelper.AddressSplitRow(maxAddress[1]);
                    studentIds = worksheet.Cells[2, 1, maxRow, 1].Select(c => c.Value.ToString());
                    foreach (var studnetId in studentIds)
                    {
                        if (studnetId == null) continue;
                        try
                        {
                            var user = CreateUser();
                            user.StudentId = studnetId;
                            await _userStore.SetUserNameAsync(user, studnetId, CancellationToken.None);
                            var result = await _userManager.CreateAsync(user, defaultPassword);
                            if (result.Succeeded)
                            {
                                _logger.LogInformation("User created a new account with password.");
                                if ((await _userManager.AddToRoleAsync(user, "reader")).Succeeded)
                                    _logger.LogInformation("User add to reader.");
                            }
                            else
                            {
                                foreach (var e in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, e.Description);
                                }
                            }
                        }
                        catch (Exception e) { ModelState.AddModelError(string.Empty, e.Message); }
                    }
                }
            }
            return Page();
        }

        private StudentUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<StudentUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(StudentUser)}'. " +
                    $"Ensure that '{nameof(StudentUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
