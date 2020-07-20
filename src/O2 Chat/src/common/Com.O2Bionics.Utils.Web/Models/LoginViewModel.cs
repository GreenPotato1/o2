using System.ComponentModel.DataAnnotations;

namespace Com.O2Bionics.Utils.Web.Models
{
    public class LoginViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Required]
        public string LocalDate { get; set; }
    }
}