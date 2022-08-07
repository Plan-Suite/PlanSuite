using PlanSuite.Models.Persistent;
using Stripe;

namespace PlanSuite.Utility
{
    public static class PaymentUtils
    {
        public static async Task<Customer> CreateCustomerAsync(ApplicationUser user)
        {
            var options = new CustomerCreateOptions
            {
                Email = user.Email
            };

            var service = new CustomerService();
            return await service.CreateAsync(options);
        }
    }
}
