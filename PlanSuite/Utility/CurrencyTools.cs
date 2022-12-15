using Microsoft.CodeAnalysis;
using Stripe.Checkout;
using System.Globalization;

namespace PlanSuite.Utility
{
    public static class CurrencyTools
    {
        private static List<string> m_CurrencySymbols;

        static CurrencyTools()
        {
            m_CurrencySymbols = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(culture => culture.NumberFormat.CurrencySymbol)
                .Where(symbol => symbol.Length == 1)
                .Distinct()
                .ToList();
        }

        public static List<string> GetAllSymbols()
        {
            List<string> currencyList = new List<string>();
            foreach(var currency in m_CurrencySymbols)
            {
                Console.WriteLine($"getting {currency}");
                currencyList.Add(currency);
            }
            return currencyList;
        }
    }
}
