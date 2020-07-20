using System.ComponentModel.DataAnnotations;

namespace Com.O2Bionics.ChatService.Web.Console.Models.TestO2BionicsMock
{
    public class ChatSourceViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "O2Bionics UserId")]
        public int O2BionicsUserId { get; set; }

        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Can contain digits only")]
        [Display(Name = "O2Bionics AccountNumber")]
        public string O2BionicsAccountNumber { get; set; }
    }
}