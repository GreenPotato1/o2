using System.ComponentModel.DataAnnotations;

namespace Com.O2Bionics.ChatService.Web.Console.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}