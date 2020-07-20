using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Web.Console.Controllers;
using Com.O2Bionics.Utils.Web;
using Com.O2Bionics.Utils.Web.DataAnnotation;

namespace Com.O2Bionics.ChatService.Web.Console.Models.Account
{
    public class RegisterCustomerViewModel
    {
        [Required]
        [MinLength(1)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        [Remote(nameof(AccountController.UserAlreadyExists), "Account", ErrorMessage = "User with this Email already exists", HttpMethod = "POST", AdditionalFields = LoginConstants.TokenKey)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name="Customer Name")]
        [MinLength(1)]
        public string CustomerName { get; set; }

        [Required]
        [Display(Name = "Site Url", Description = "Your Site Url")]
        [Url]
        public string WebSiteAddress { get; set; }

        [MustBeTrue(ErrorMessage = "You should tick this to join the O2Chat")]
        public bool AgreedToTermsOfService { get; set; }

        public string O2BionicsAccountData { get; set; }

        [Required]
        public string LocalDate { get; set; }
    }
}