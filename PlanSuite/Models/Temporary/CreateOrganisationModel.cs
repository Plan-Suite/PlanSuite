using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class CreateOrganisationModel
    {
        public CreateOrganisationModel()
        {

        }

        [Required(ErrorMessage = "You must enter a valid organisation name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You must enter a valid organisation description")]
        public string Description { get; set; }

        public Guid OwnerId { get; set; }
    }
}
