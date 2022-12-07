using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
}
