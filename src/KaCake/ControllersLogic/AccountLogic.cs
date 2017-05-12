using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using KaCake.Data.Models;
using KaCake.Services;
using KaCake.ViewModels.Account;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KaCake.ControllersLogic
{
    [Authorize]
    public class AccountLogic
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        public AccountLogic(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountLogic>();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, ModelStateDictionary modelState, string returnUrl = null)
        {
            model.ReturnURL = returnUrl;
            if (modelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return new ObjectResult(new { Code = 200, Message = "Success", ReturnUrl = returnUrl, Result = result });
                }
                else
                {
                    modelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return new ObjectResult(new { Code = 400,  Message = "Login failed", LoginData = model, Result = result });
                }
            }

            // If we got this far, something failed, redisplay form
            return new ObjectResult(new { Code = 400, Message = "Invalid login data", LoginData = model });
        }
    }
}
