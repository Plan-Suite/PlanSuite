using Stripe;

namespace PlanSuite.Services
{
    public class PaymentService
    {
        public static string Secret { get; private set; }
        public static string PlusUrl { get; private set; }
        public static string ProUrl { get; private set; }

        public static void InitPaymentService(string apiKey, string secret, string plus, string pro)
        {
            Console.WriteLine($"Configuring payment service...");

            StripeConfiguration.ApiKey = apiKey;
            Secret = secret;
            PlusUrl = plus;
            ProUrl = pro;

            Console.WriteLine($"Payment service configured for Stripe.");
        }
    }
}
