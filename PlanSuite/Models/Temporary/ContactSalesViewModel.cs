using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PlanSuite.Models.Temporary
{
    public class ContactSalesViewModel : BaseViewModel
    {
        public ContactSalesModel ContactSales { get; set; } = new ContactSalesModel();
        public List<string> Countries { get; set; } = new List<string>();

        public ContactSalesViewModel()
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach(var culture in cultures)
            {
                try
                {
                    RegionInfo region = new RegionInfo(culture.LCID);
                    if(!Countries.Contains(region.EnglishName))
                    {
                        Countries.Add(region.EnglishName);
                        Console.WriteLine($"Added region {region.EnglishName}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception during cultures loop in ContactSalesViewModel: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        public class ContactSalesModel
        {
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Display(Name = "Job Title")]
            public string JobTitle { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            public string Message { get; set; }

            [Required]
            public string Token { get; set; }
        }
    }

    public class SalesContactedModel : BaseViewModel
    {
        public string FirstName { get; set; }
    }
}
