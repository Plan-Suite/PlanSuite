// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<ResetPasswordModel> m_Logger;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, ApplicationDbContext database, ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            m_Database = database;
            m_Logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            public string Code { get; set; }

        }

        public async Task<IActionResult> OnGetAsync(string code = null)
        {
            if (code == null)
            {
                m_Logger.LogError("ResetPassword: Code was null.");
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var request = m_Database.PasswordResetRequests.Where(r => r.Code.Equals(code)).FirstOrDefault();
                if(request == null || request.Expiry < DateTime.Now)
                {
                    m_Logger.LogError("ResetPassword: Request was null, or request had expired.");
                    return BadRequest("An invalid or expired password reset code was provided.");
                }

                var user = await _userManager.FindByIdAsync(request.AccountId.ToString());
                if (user == null)
                {
                    m_Logger.LogError("ResetPassword: Invalid user.");
                    return BadRequest("Invalid user for request.");
                }


                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                    Email = user.Email
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                m_Logger.LogError("ModelState.IsValid false during OnPostAsync in ResetPassword");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                m_Logger.LogError("User did not exist when attempting to reset password");
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            m_Logger.LogInformation($"ResetPasswordAsync called for {user.FullName}");
            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                m_Logger.LogInformation($"ResetPasswordAsync success for {user.FullName}");

                var request = m_Database.PasswordResetRequests.Where(r => r.Code.Equals(Input.Code, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (request != null)
                {
                    m_Logger.LogInformation($"Removing password reset request {request.Id} on use");
                    m_Database.Remove(request);
                    await m_Database.SaveChangesAsync();
                }

                return RedirectToPage("./ResetPasswordConfirmation");
            }

            m_Logger.LogError($"ResetPasswordAsync FAILED for {user.FullName}");
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
