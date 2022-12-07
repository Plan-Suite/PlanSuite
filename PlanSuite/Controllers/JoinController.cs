using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;
using Stripe;
using Stripe.Checkout;

namespace PlanSuite.Controllers
{
    public class JoinController : Controller
    {
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly SignInManager<ApplicationUser> m_SignInManager;
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<JoinController> m_Logger;

        public JoinController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext database, ILogger<JoinController> logger)
        {
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Database = database;
            m_Logger = logger;
        }

        [Route("welcome")]
        public async Task<IActionResult> Welcome()
        {
            WelcomePageModel model = new WelcomePageModel();
            if(!m_SignInManager.IsSignedIn(User))
            {
                // Redirect the user to the sign in page.
                return Redirect("/Identity/Account/Login");
            }

            var user = await m_UserManager.GetUserAsync(User);
            if(user == null)
            {
                // User has returned null, this should never happen in 99.9% of cases.
                return NotFound("User returned null, this should not happen.");
            }

            m_Logger.LogInformation($"Showing first time login page for {user.UserName}");
            if (user.FinishedFirstTimeLogin == true)
            {
                // User has already completed their first time login, so we're just gonna redirect them to the index page.
                return Redirect("/Home/Index");
            }

            return View(model);
        }

        [Route("upgrade")]
        public async Task<IActionResult> Upgrade()
        {
            WelcomePageModel model = new WelcomePageModel();
            if (!m_SignInManager.IsSignedIn(User))
            {
                // Redirect the user to the sign in page.
                return Redirect("/Identity/Account/Login");
            }

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                // User has returned null, this should never happen in 99.9% of cases.
                return NotFound("User returned null, this should not happen.");
            }

            return View(model);
        }

        public async Task<IActionResult> ContinueWithFree()
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                // Redirect the user to the sign in page.
                return Redirect("/Identity/Account/Login");
            }

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                // User has returned null, this should never happen.
                return NotFound("User returned null, this should not happen.");
            }

            if (user.FinishedFirstTimeLogin == true)
            {
                // User has already completed their first time login, so we're just gonna redirect them to the index page.
                return Redirect("/Home/Index");
            }

            m_Logger.LogInformation($"Setting free tier for {user.UserName}");
            user.PaymentTier = PaymentTier.Free;
            user.FinishedFirstTimeLogin = true;
            await m_UserManager.UpdateAsync(user);

            return Redirect("/Home/Index");
        }

        public async Task<IActionResult> UpgradePlan()
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                // Redirect the user to the sign in page.
                return Redirect("/Identity/Account/Login");
            }

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                // User has returned null, this should never happen.
                return NotFound("User returned null, this should not happen.");
            }

            var customerService = new CustomerService();
            Customer customer = await CreateUserStripeCustomer(user, customerService);

            customer = await customerService.GetAsync(user.StripeCustomerId);
            if (customer == null)
            {
                customer = await PaymentUtils.CreateCustomerAsync(user);

                user.StripeCustomerId = customer.Id;
                await m_UserManager.UpdateAsync(user);
            }

            string domain = $"https://{HttpContext.Request.Host}";
            string lookupKey = Request.Form["lookup_key"];
            m_Logger.LogInformation($"Domain is {domain}");

            PaymentTier paymentTier = PaymentTier.Plus;
            if (lookupKey == PaymentService.ProUrl)
            {
                paymentTier = PaymentTier.Pro;
            }

            // Create sale object
            Sale sale = new Sale
            {
                SaleDate = DateTime.Now,
                OwnerId = Guid.Parse(user.Id),
                PaymentTier = paymentTier,
                SaleState = SaleState.Pending
            };

            await m_Database.Sales.AddAsync(sale);
            await m_Database.SaveChangesAsync();

            m_Logger.LogInformation($"Create prices");
            var priceOptions = new PriceListOptions
            {
                LookupKeys = new List<string>
                {
                    lookupKey
                }
            };

            var priceService = new PriceService();
            StripeList<Price> prices = priceService.List(priceOptions);

            m_Logger.LogInformation($"Create session options {prices.Data.Count}");
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = lookupKey,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = domain + "/api/Payment/Success?sessionId={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/api/Payment/Cancel",
                AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
                ClientReferenceId = sale.Id.ToString()
            };

            m_Logger.LogInformation("Create session");
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        private async Task<Customer> CreateUserStripeCustomer(ApplicationUser user, CustomerService customerService)
        {
            Customer customer;
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                m_Logger.LogInformation($"Creating stripe customer for user {user.Id}");
                customer = await PaymentUtils.CreateCustomerAsync(user);

                Console.WriteLine(customer.ToJson());

                user.StripeCustomerId = customer.Id;
                await m_UserManager.UpdateAsync(user);
            }
            else
            {
                customer = await customerService.GetAsync(user.StripeCustomerId);
                if (customer == null)
                {
                    customer = await PaymentUtils.CreateCustomerAsync(user);

                    Console.WriteLine(customer.ToJson());

                    user.StripeCustomerId = customer.Id;
                    await m_UserManager.UpdateAsync(user);
                }
            }

            return customer;
        }

        [Route("upgraded/{saleId}&{amount}")]
        public IActionResult UpgradeSuccess(int saleId, long amount)
        {
            UpgradeSuccessModel model = new UpgradeSuccessModel();

            var sale = m_Database.Sales.Where(s => s.Id == saleId).FirstOrDefault();
            if (sale == null)
            {
                m_Logger.LogWarning($"No sale for saleId {saleId} found.");
                return Redirect("/Home/Index");
            }

            model.Sale = sale;
            model.Amount = (int)amount;

            return View(model);
        }

        public async Task<IActionResult> ContinueRegistration(FinishRegistrationModel.FinishRegistrationInputModel input)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(input));
            if(!ModelState.IsValid)
            {
                return BadRequest("ModelState invalid");
            }


            ApplicationUser user = await m_UserManager.FindByIdAsync(input.UserId.ToString());
            if(user == null)
            {
                return BadRequest("User was null during registration");
            }

            var result = await m_UserManager.AddPasswordAsync(user, input.Password);
            if (!result.Succeeded)
            {
                return BadRequest("Cannot assign password");
            }

            user.FirstName = input.FirstName;
            user.LastName = input.LastName;
            await m_UserManager.UpdateAsync(user);

            return RedirectToAction(nameof(Welcome));
        }

        public async Task<IActionResult> FinishRegistration()
        {
            ApplicationUser user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User was null during registration");
            }

            FinishRegistrationModel viewModel = new FinishRegistrationModel();
            viewModel.UserId = Guid.Parse(user.Id);
            viewModel.Email = user.Email;

            return View(viewModel);
        }

        public static void DoFinishedRegistrationChecks(ControllerBase controllerBase, ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.FirstName))
            {
                controllerBase.Redirect("/Join/FinishRegistration");
            }
        }
    }
}
