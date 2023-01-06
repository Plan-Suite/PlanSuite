using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Enums
{
    public enum Industry
    {
        /// <summary>
        /// Arts, Entertainment and Recreation
        /// </summary>
        Entertainment,

        /// <summary>
        /// Mining and Quarrying
        /// </summary>
        Mining,

        /// <summary>
        /// Hospitality
        /// </summary>
        Hospitality,

        /// <summary>
        /// Utility Services
        /// </summary>
        Utilities,

        /// <summary>
        /// Real Estate
        /// </summary>
        [Display(Name = "Real Estate")] RealEstate,

        /// <summary>
        /// Media
        /// </summary>
        Media,

        /// <summary>
        /// Health Care and Social Work
        /// </summary>
        [Display(Name = "Health and Social Care")] HealthCare,

        /// <summary>
        /// Construction
        /// </summary>
        Construction,

        /// <summary>
        /// Education
        /// </summary>
        Education,

        /// <summary>
        /// Financial Services
        /// </summary>
        [Display(Name = "Financial Services")] FinancalServices,

        /// <summary>
        /// Agriculture
        /// </summary>
        Agriculture,

        /// <summary>
        /// Transport
        /// </summary>
        Transport,

        /// <summary>
        /// Science and Technology
        /// </summary>
        [Display(Name = "Science & Technology")] ScienceTechnology,

        /// <summary>
        /// Manufacturing
        /// </summary>
        Manufacturing,

        /// <summary>
        /// Information Technology
        /// </summary>
        [Display(Name = "Information Technology")] InformationTechnology,

        /// <summary>
        /// Defence Sector
        /// </summary>
        Defence,

        /// <summary>
        /// Public Sector
        /// </summary>
        [Display(Name = "Public Sector")] PublicSector,

        /// <summary>
        /// Other
        /// </summary>
        Other,
    }
}
