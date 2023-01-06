using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static PlanSuite.Models.Temporary.FinishRegistrationModel;

namespace PlanSuite.Models.Temporary
{
    public class FinishRegistrationModel
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public FinishRegistrationInputModel Input { get; set; } = new FinishRegistrationInputModel();

        public class FinishRegistrationInputModel
        {
            public Guid UserId { get; set; }

            [DisplayName("First Name")]
            [DataType(DataType.Text)]
            public string FirstName { get; set; }

            [DisplayName("Last Name")]
            [DataType(DataType.Text)]
            public string LastName { get; set; }

            [DisplayName("Password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DisplayName("Confirm Password")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }

    public class CreateOrganisationRegistrationModel
    {
        public Guid UserId { get; set; }
        public CreateOrganisationRegistrationInputModel Input { get; set; } = new CreateOrganisationRegistrationInputModel();

        public class CreateOrganisationRegistrationInputModel
        {
            public Guid UserId { get; set; }

            [DisplayName("Organisation Name")]
            [DataType(DataType.Text)]
            public string OrganisationName { get; set; }

            [DisplayName("Organisation Description")]
            [DataType(DataType.Text)]
            public string Description { get; set; }

            [DisplayName("Industry")]
            public int Industry { get; set; }
        }
    }
}
