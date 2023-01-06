// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> m_SignInManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            m_SignInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public InputModel Input { get; set; }

        public class InputModel
        {
            public Guid User { get; set; }
            public string Token { get; set; }

            [Required]
            [MinLength(2, ErrorMessage = "The {0} must be at least {1} characters long.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [MinLength(2, ErrorMessage = "The {0} must be at least {1} characters long.")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-z].*[a-z].*[a-z]).{8,20}$",
                    ErrorMessage = "Password is not valid, it must be between 8 - 20 characters and contain a number and a capital letter")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public ApplicationUser AppUser { get; set; }
        public string Code { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            if (user.EmailConfirmed == true)
            {
                return RedirectToAction("FinishRegistration", "Join");
            }

            user.RegistrationDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            string purpose = UserManager<ApplicationUser>.ConfirmEmailTokenPurpose;
            string provider = _userManager.Options.Tokens.EmailConfirmationTokenProvider;
            bool valid = await _userManager.VerifyUserTokenAsync(user, provider, purpose, code);
            if(!valid)
            {
                return BadRequest("Invalid token provided");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                return BadRequest("Cannot confirm email");
            }
            await m_SignInManager.SignInAsync(user, isPersistent: false);
            AppUser = user;
            return RedirectToAction("FinishRegistration", "Join");
        }
    }
}
